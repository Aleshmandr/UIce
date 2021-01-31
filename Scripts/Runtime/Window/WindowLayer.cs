﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Juice
{
	public class WindowLayer : Layer<IWindow, WindowShowSettings, WindowHideSettings>
	{
		public delegate void WindowChangeHandler(IWindow oldWindow, IWindow newWindow);

		public event WindowChangeHandler CurrentWindowChanged;

		public IWindow CurrentWindow
		{
			get => currentWindow;

			private set
			{
				IWindow oldWindow = currentWindow;
				currentWindow = value;
				OnCurrentWindowChanged(oldWindow, currentWindow);
			}
		}

		[SerializeField] private WindowParaLayer priorityParaLayer = null;

		private readonly Queue<WindowHistoryEntry> windowQueue = new Queue<WindowHistoryEntry>();
		private readonly Stack<WindowHistoryEntry> windowHistory = new Stack<WindowHistoryEntry>();
		private IWindow currentWindow;

		protected virtual void OnEnable()
		{
			if (priorityParaLayer != null)
			{
				priorityParaLayer.BackgroundClicked -= OnPopupsBackgroundClicked;
				priorityParaLayer.BackgroundClicked += OnPopupsBackgroundClicked;
			}
		}

		internal void SetPriorityLayer(WindowParaLayer priorityParaLayer)
		{
			this.priorityParaLayer = priorityParaLayer;

			priorityParaLayer.BackgroundClicked -= OnPopupsBackgroundClicked;
			priorityParaLayer.BackgroundClicked += OnPopupsBackgroundClicked;
		}

		public override async Task HideAll()
		{
			Task[] tasks = new Task[registeredViews.Count];
			int i = 0;

			foreach (KeyValuePair<Type, IWindow> current in registeredViews)
			{
				tasks[i] = HideWindow(current.Value, null);
				i++;
			}

			await Task.WhenAll(tasks);

			CurrentWindow = null;
			priorityParaLayer.RefreshBackground();
			windowHistory.Clear();
		}

		public override void ReparentView(IView view, Transform viewTransform)
		{
			IWindow window = view as IWindow;
			bool doBaseReparent = true;

			if (window == null)
			{
				Debug.LogError($"View {viewTransform.name} is not a Window!");
			}
			else
			{
				if (window.IsPopup)
				{
					priorityParaLayer.AddView(viewTransform);
					doBaseReparent = false;
				}
			}

			if (doBaseReparent)
			{
				base.ReparentView(view, viewTransform);
			}
		}

		protected virtual void OnCurrentWindowChanged(IWindow oldWindow, IWindow newWindow)
		{
			CurrentWindowChanged?.Invoke(oldWindow, newWindow);
		}

		protected override void ProcessViewRegister(IWindow view)
		{
			base.ProcessViewRegister(view);

			view.CloseRequested += OnCloseRequestedByWindow;
		}

		protected override void ProcessViewUnregister(IWindow view)
		{
			base.ProcessViewUnregister(view);

			view.CloseRequested -= OnCloseRequestedByWindow;
		}

		protected override async Task ShowView(IWindow view, WindowShowSettings settings)
		{
			if (ShouldEnqueue(view, settings))
			{
				EnqueueWindow(view, settings);
			}
			else
			{
				await ShowInForeground(view, settings);
			}
		}

		protected override async Task HideView(IWindow view, WindowHideSettings settings)
		{
			if (view == CurrentWindow)
			{
				windowHistory.Pop();

				if (view.IsPopup && !NextWindowIsPopup())
				{
					priorityParaLayer.HideBackground();
				}

				IWindow windowToClose = view;
				IWindow windowToOpen = GetNextWindow();

				if (windowToOpen == null)
				{
					CurrentWindow = null;
				}

				if (windowToClose == windowToOpen)
				{
					await HideWindow(windowToClose, settings?.OutTransition);
					await ShowNextWindow();
				}
				else
				{
					await Task.WhenAll(
						HideWindow(windowToClose, settings?.OutTransition),
						ShowNextWindow());
				}
			}
			else
			{
				Debug.LogErrorFormat
				(
					"Hide requested on Window {0} but that's not the currently open one ({1})! Ignoring request.",
					view.GetType().Name,
					CurrentWindow != null ? CurrentWindow.GetType().Name : "current is null"
				);
			}
		}

		private static WindowHideSettings BuildEmptyHideSettings(IView controller)
		{
			return new WindowHideSettings(controller.GetType(), null);
		}

		private bool NextWindowIsPopup()
		{
			bool nextWindowInQueueIsPopup = windowQueue.Count > 0 && windowQueue.Peek().View.IsPopup;
			bool lastWindowInHistoryIsPopup = windowHistory.Count > 0 && windowHistory.Peek().View.IsPopup;

			return nextWindowInQueueIsPopup || (windowQueue.Count == 0 && lastWindowInHistoryIsPopup);
		}

		private IWindow GetNextWindow()
		{
			IWindow result = null;

			if (windowQueue.Count > 0)
			{
				result = windowQueue.Peek().View;
			}
			else if (windowHistory.Count > 0)
			{
				result = windowHistory.Peek().View;
			}

			return result;
		}

		private async Task HideWindow(IWindow window, Transition overrideTransition = null)
		{
			await window.Hide(overrideTransition);
		}

		private async Task ShowNextWindow()
		{
			if (windowQueue.Count > 0)
			{
				await ShowNextInQueue();
			}
			else if (windowHistory.Count > 0)
			{
				await ShowPreviousInHistory();
			}
		}

		private async Task ShowNextInQueue()
		{
			if (windowQueue.Count > 0)
			{
				WindowHistoryEntry entry = windowQueue.Dequeue();

				await ShowWindow(entry);
			}
		}

		private async Task ShowPreviousInHistory()
		{
			if (windowHistory.Count > 0)
			{
				WindowHistoryEntry window = windowHistory.Pop();

				await ShowWindow(window);
			}
		}

		private void OnCloseRequestedByWindow(IView controller)
		{
			uiFrame.HideWindow(BuildEmptyHideSettings(controller)).RunAndForget();
		}

		private void OnPopupsBackgroundClicked()
		{
			if (CurrentWindow != null && CurrentWindow.IsPopup && CurrentWindow.CloseOnShadowClick)
			{
				uiFrame.HideWindow(BuildEmptyHideSettings(CurrentWindow)).RunAndForget();
			}
		}

		private bool ShouldEnqueue(IWindow window, WindowShowSettings settings)
		{
			WindowPriority priority = settings?.Priority ?? window.WindowPriority;

			return priority != WindowPriority.ForceForeground
			       && (CurrentWindow != null || windowQueue.Count > 0);
		}

		private void EnqueueWindow(IWindow window, WindowShowSettings settings)
		{
			windowQueue.Enqueue(new WindowHistoryEntry(window, settings));
		}

		private async Task ShowInForeground(IWindow window, WindowShowSettings settings)
		{
			await ShowInForeground(new WindowHistoryEntry(window, settings));
		}

		private async Task ShowInForeground(WindowHistoryEntry windowEntry)
		{
			if (CurrentWindow == windowEntry.View)
			{
				Debug.LogWarning($"[WindowLayer] The requested ({CurrentWindow.GetType().Name}) is already open!" +
				                 " This will add a duplicate to the history and might cause inconsistent behaviour." +
				                 " It is recommended that if you need to open the same view multiple times" +
				                 " (eg: when implementing a warning message pop-up), it closes itself upon the player input" +
				                 " that triggers the continuation of the flow.");
			}

			if (CurrentWindow != windowEntry.View
			    && CurrentWindow != null
			    && CurrentWindow.HideOnForegroundLost
			    && !windowEntry.View.IsPopup)
			{
				HideWindow(CurrentWindow, windowEntry.Settings.OutTransition).RunAndForget();
			}

			await ShowWindow(windowEntry);
		}

		private async Task ShowWindow(WindowHistoryEntry windowEntry)
		{
			if (windowEntry.View.IsPopup)
			{
				priorityParaLayer.ShowBackground();
			}

			windowHistory.Push(windowEntry);
			CurrentWindow = windowEntry.View;

			await windowEntry.Show();
		}
	}
}
