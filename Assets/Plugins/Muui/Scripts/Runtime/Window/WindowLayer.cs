﻿using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Muui
{
#pragma warning disable 4014
	public class WindowLayer : BaseLayer<IWindowController>
	{
		public delegate void WindowLayerDelegate();

		public event WindowLayerDelegate RequestScreenBlock;
		public event WindowLayerDelegate RequestScreenUnblock;

		public IWindowController CurrentWindow { get; private set; }

		[SerializeField] private WindowParaLayer priorityParaLayer = null;

		private Queue<WindowHistoryEntry> windowQueue = new Queue<WindowHistoryEntry>();
		private Stack<WindowHistoryEntry> windowHistory = new Stack<WindowHistoryEntry>();
		private HashSet<IScreenController> screensTransitioning = new HashSet<IScreenController>();
		private bool IsScreenTransitionInProgress => screensTransitioning.Count > 0;

		protected virtual void OnEnable()
		{
			if (priorityParaLayer != null)
			{
				priorityParaLayer.ShadowClicked -= PopupsShadowClicked;
				priorityParaLayer.ShadowClicked += PopupsShadowClicked;
			}
		}

		internal void SetPriorityWindow(WindowParaLayer priorityParaLayer)
		{
			this.priorityParaLayer = priorityParaLayer;

			priorityParaLayer.ShadowClicked -= PopupsShadowClicked;
			priorityParaLayer.ShadowClicked += PopupsShadowClicked;
		}

		public override Task ShowScreen(IWindowController screen)
		{
			return ShowScreen<IWindowProperties>(screen, null);
		}

		public override Task ShowScreen<TProps>(IWindowController screen, TProps properties)
		{
			Task result;
			IWindowProperties windowProperties = properties as WindowProperties;

			if (ShouldEnqueue(screen, windowProperties))
			{
				EnqueueWindow(screen, windowProperties);
				result = Task.CompletedTask;
			}
			else
			{
				result = DoShow(screen, windowProperties);
			}

			return result;
		}

		public override async Task HideScreen(IWindowController screen)
		{
			if (screen == CurrentWindow)
			{
				windowHistory.Pop();
				AddTransition(screen);

				if (screen.IsPopup && NextWindowIsPopup() == false)
				{
					priorityParaLayer.HideBackgroundShadow();
				}

				await screen.Hide();

				CurrentWindow = null;

				if (windowQueue.Count > 0)
				{
					await ShowNextInQueue();
				}
				else if (windowHistory.Count > 0)
				{
					await ShowPreviousInHistory();
				}
			}
			else
			{
				Debug.LogErrorFormat
				(
					"Hide requested on Window {0} but that's not the currently open one ({1})! Ignoring request.",
					screen.GetType().Name,
					CurrentWindow != null ? CurrentWindow.GetType().Name : "current is null"
				);
			}
		}

		public override async Task HideAll(bool animate = true)
		{
			await base.HideAll(animate);

			CurrentWindow = null;
			priorityParaLayer.RefreshDarken();
			windowHistory.Clear();
		}

		public override void ReparentScreen(IScreenController controller, Transform screenTransform)
		{
			IWindowController window = controller as IWindowController;
			bool doBaseReparent = true;

			if (window == null)
			{
				Debug.LogError($"Screen {screenTransform.name} is not a Window!");
			}
			else
			{
				if (window.IsPopup)
				{
					priorityParaLayer.AddScreen(screenTransform);
					doBaseReparent = false;
				}
			}

			if (doBaseReparent)
			{
				base.ReparentScreen(controller, screenTransform);
			}
		}

		protected override void ProcessScreenRegister(IWindowController controller)
		{
			base.ProcessScreenRegister(controller);

			controller.InTransitionFinished += OnInAnimationFinished;
			controller.OutTransitionFinished += OnOutAnimationFinished;
			controller.CloseRequested += OnCloseRequestedByWindow;
		}

		protected override void ProcessScreenUnregister(IWindowController controller)
		{
			base.ProcessScreenUnregister(controller);

			controller.InTransitionFinished -= OnInAnimationFinished;
			controller.OutTransitionFinished -= OnOutAnimationFinished;
			controller.CloseRequested -= OnCloseRequestedByWindow;
		}

		private void OnInAnimationFinished(IScreenController controller)
		{
			RemoveTransition(controller);
		}

		private void OnOutAnimationFinished(IScreenController controller)
		{
			RemoveTransition(controller);
			IWindowController window = controller as IWindowController;

			if (window.IsPopup)
			{
				priorityParaLayer.RefreshDarken();
			}
		}

		private void OnCloseRequestedByWindow(IScreenController controller)
		{
			HideScreen(controller as IWindowController);
		}

		private void PopupsShadowClicked()
		{
			if (CurrentWindow != null && CurrentWindow.IsPopup && CurrentWindow.CloseOnShadowClick)
			{
				HideScreen(CurrentWindow);
			}
		}

		private bool ShouldEnqueue(IWindowController window, IWindowProperties properties)
		{
			bool result = false;

			if (CurrentWindow == null && windowQueue.Count == 0)
			{
				result = false;
			}
			else if (properties != null && properties.SupressPrefabProperties)
			{
				result = properties.WindowQueuePriority != WindowPriority.ForceForeground;
			}
			else if (window.WindowPriority != WindowPriority.ForceForeground)
			{
				result = true;
			}

			return result;
		}

		private void EnqueueWindow(IWindowController window, IWindowProperties properties)
		{
			windowQueue.Enqueue(new WindowHistoryEntry(window, properties));
		}

		private async Task DoShow(WindowHistoryEntry windowEntry)
		{
			if (CurrentWindow == windowEntry.Screen)
			{
				Debug.LogWarning(
					string.Format(
						"[WindowUILayer] The requested WindowId ({0}) is already open! This will add a duplicate to the " +
						"history and might cause inconsistent behaviour. It is recommended that if you need to open the same" +
						"screen multiple times (eg: when implementing a warning message pop-up), it closes itself upon the player input" +
						"that triggers the continuation of the flow."
						, CurrentWindow.GetType().Name));
			}
			else if (CurrentWindow != null && CurrentWindow.HideOnForegroundLost && !windowEntry.Screen.IsPopup)
			{
				CurrentWindow.Hide();
			}

			windowHistory.Push(windowEntry);
			AddTransition(windowEntry.Screen);

			if (windowEntry.Screen.IsPopup)
			{
				priorityParaLayer.ShowBackgroundShadow();
			}

			await windowEntry.Show();

			CurrentWindow = windowEntry.Screen;
		}

		private Task DoShow(IWindowController window, IWindowProperties properties)
		{
			return DoShow(new WindowHistoryEntry(window, properties));
		}

		private void AddTransition(IScreenController screen)
		{
			screensTransitioning.Add(screen);
			RequestScreenBlock?.Invoke();
		}

		private void RemoveTransition(IScreenController screen)
		{
			screensTransitioning.Remove(screen);

			if (IsScreenTransitionInProgress == false)
			{
				RequestScreenUnblock?.Invoke();
			}
		}

		private bool NextWindowIsPopup()
		{
			bool nextWindowInQueueIsPopup = windowQueue.Count > 0 && windowQueue.Peek().Screen.IsPopup;
			bool lastWindowInHistoryIsPopup = windowHistory.Count > 0 && windowHistory.Peek().Screen.IsPopup;

			return nextWindowInQueueIsPopup || (windowQueue.Count == 0 && lastWindowInHistoryIsPopup);
		}

		private async Task ShowNextInQueue()
		{
			if (windowQueue.Count > 0)
			{
				WindowHistoryEntry window = windowQueue.Dequeue();
				await DoShow(window);
			}
		}

		private async Task ShowPreviousInHistory()
		{
			if (windowHistory.Count > 0)
			{
				WindowHistoryEntry window = windowHistory.Pop();
				await DoShow(window);
			}
		}
	}
#pragma warning restore 4014
}
