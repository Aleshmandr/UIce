﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Juice
{
	public class UIFrame : MonoBehaviour
	{
		public delegate void PanelOperationHandler(IPanel panel);

		public event WindowOpenHandler WindowOpening;
		public event WindowOpenHandler WindowOpened;
		public event WindowCloseHandler WindowClosing;
		public event WindowCloseHandler WindowClosed;
		public event PanelOperationHandler PanelOpening;
		public event PanelOperationHandler PanelOpened;
		public event PanelOperationHandler PanelClosing;
		public event PanelOperationHandler PanelClosed;

		public Canvas MainCanvas
		{
			get
			{
				if (mainCanvas == null)
				{
					mainCanvas = GetComponentInChildren<Canvas>();
				}

				return mainCanvas;
			}
		}

		public Camera UICamera => MainCanvas.worldCamera;

		public IWindow CurrentWindow => windowLayer.CurrentWindow;

		[SerializeField] private bool initializeOnAwake;

		private Canvas mainCanvas;
		private PanelLayer panelLayer;
		private WindowLayer windowLayer;
		private GraphicRaycaster graphicRaycaster;

		private readonly Dictionary<Type, IView> registeredViews = new Dictionary<Type, IView>();
		private readonly HashSet<IView> viewsInTransition = new HashSet<IView>();

		private void Reset()
		{
			initializeOnAwake = true;
		}

		private void Awake()
		{
			if (initializeOnAwake)
			{
				Initialize();
			}
		}

		public virtual void Initialize()
		{
			if (panelLayer == null)
			{
				panelLayer = GetComponentInChildren<PanelLayer>();

				if (panelLayer == null)
				{
					Debug.LogError("UI Frame lacks Panel Layer!");
				}
				else
				{
					panelLayer.PanelOpening += OnPanelOpening;
					panelLayer.PanelOpened += OnPanelOpened;
					panelLayer.PanelClosing += OnPanelClosing;
					panelLayer.PanelClosed += OnPanelClosed;
					panelLayer.Initialize(this);
				}
			}

			if (windowLayer == null)
			{
				windowLayer = GetComponentInChildren<WindowLayer>();

				if (windowLayer == null)
				{
					Debug.LogError("UI Frame lacks Window Layer!");
				}
				else
				{
					windowLayer.WindowOpening += OnWindowOpening;
					windowLayer.WindowOpened += OnWindowOpened;
					windowLayer.WindowClosing += OnWindowClosing;
					windowLayer.WindowClosed += OnWindowClosed;
					windowLayer.Initialize(this);
				}
			}

			graphicRaycaster = MainCanvas.GetComponent<GraphicRaycaster>();
		}

		public void RegisterView<T>(T view) where T : IView
		{
			if (IsViewValid(view))
			{
				Type viewType = view.GetType();

				if (typeof(IPanel).IsAssignableFrom(viewType))
				{
					IPanel viewAsPanel = view as IPanel;
					ProcessViewRegister(viewAsPanel, panelLayer);
				}
				else if (typeof(IWindow).IsAssignableFrom(viewType))
				{
					IWindow viewAsWindow = view as IWindow;
					ProcessViewRegister(viewAsWindow, windowLayer);
				}
				else
				{
					Debug.LogError($"The View type {typeof(T).Name} must implement {nameof(IPanel)} or {nameof(IWindow)}.");
				}
			}
		}

		public void UnregisterView(Type viewType)
		{
			if (registeredViews.TryGetValue(viewType, out IView view))
			{
				Component viewAsComponent = view as Component;

				if (viewAsComponent != null)
				{
					viewAsComponent.gameObject.SetActive(false);
					viewAsComponent.transform.SetParent(null);
				}

				registeredViews.Remove(viewType);
			}
		}
		
		public void UnregisterView<T>() where T : IView
		{
			UnregisterView(typeof(T));
		}
		
		public void ShowPanel<T>(IViewModel viewModel, PanelOptions overrideOptions = null) where T : IPanel
		{
			ShowPanelAsync<T>(viewModel, overrideOptions).RunAndForget();
		}
		
		public async Task ShowPanelAsync<T>(IViewModel viewModel, PanelOptions overrideOptions = null) where T : IPanel
		{
			await panelLayer.ShowView(typeof(T), viewModel, overrideOptions);
		}

		public void ShowWindow<T>(IViewModel viewModel, WindowOptions overrideOptions = null) where T : IWindow
		{
			ShowWindowAsync<T>(viewModel, overrideOptions).RunAndForget();
		}
		
		public async Task ShowWindowAsync<T>(IViewModel viewModel, WindowOptions overrideOptions = null) where T : IWindow
		{
			await windowLayer.ShowView(typeof(T), viewModel, overrideOptions);
		}

		public void HidePanel<T>(PanelOptions overrideOptions = null) where T : IPanel
		{
			HidePanelAsync<T>(overrideOptions).RunAndForget();
		}
		
		public async Task HidePanelAsync<T>(PanelOptions overrideOptions = null) where T : IPanel
		{
			Type panelType = typeof(T);

			if (typeof(IPanel).IsAssignableFrom(panelType))
			{
				await panelLayer.HideView(panelType);
			}
			else
			{
				Debug.LogError($"The View type {typeof(T).Name} must implement {nameof(IPanel)}.");
			}
		}

		public void CloseCurrentWindow()
		{
			CloseCurrentWindowAsync().RunAndForget();
		}
		
		public async Task CloseCurrentWindowAsync()
		{
			if (CurrentWindow != null)
			{
				await windowLayer.HideView(CurrentWindow);
			}
		}

		public bool IsViewRegistered<T>() where T : IView
		{
			return registeredViews.ContainsKey(typeof(T));
		}

		private void OnWindowOpening(IWindow openedWindow, IWindow closedWindow, WindowOpenReason reason)
		{
			OnViewStartsTransition(openedWindow);
			WindowOpening?.Invoke(openedWindow, closedWindow, reason);
		}
		
		private void OnWindowOpened(IWindow openedWindow, IWindow closedWindow, WindowOpenReason reason)
		{
			OnViewEndsTransition(openedWindow);
			WindowOpened?.Invoke(openedWindow, closedWindow, reason);
		}

		private void OnWindowClosing(IWindow closedWindow, IWindow nextWindow, WindowHideReason reason)
		{
			OnViewStartsTransition(closedWindow);
			WindowClosing?.Invoke(closedWindow, nextWindow, reason);
		}
		
		private void OnWindowClosed(IWindow closedWindow, IWindow nextWindow, WindowHideReason reason)
		{
			OnViewEndsTransition(closedWindow);
			WindowClosed?.Invoke(closedWindow, nextWindow, reason);
		}
		
		private void OnPanelOpening(IPanel panel)
		{
			OnViewStartsTransition(panel);
			PanelOpening?.Invoke(panel);
		}
		
		private void OnPanelOpened(IPanel panel)
		{
			OnViewEndsTransition(panel);
			PanelOpened?.Invoke(panel);
		}
		
		private void OnPanelClosing(IPanel panel)
		{
			OnViewStartsTransition(panel);
			PanelClosing?.Invoke(panel);
		}
		
		private void OnPanelClosed(IPanel panel)
		{
			OnViewEndsTransition(panel);
			PanelClosed?.Invoke(panel);
		}

		private void OnViewStartsTransition(IView view)
		{
			viewsInTransition.Add(view);

			if (viewsInTransition.Count == 1)
			{
				BlockInteraction();
			}
		}

		private void OnViewEndsTransition(IView view)
		{
			viewsInTransition.Remove(view);

			if (viewsInTransition.Count <= 0)
			{
				UnblockInteraction();
			}
		}
		
		private void BlockInteraction()
		{
			if (graphicRaycaster)
			{
				graphicRaycaster.enabled = false;
			}

			foreach (var current in registeredViews)
			{
				current.Value.AllowInteraction = false;
			}
		}

		private void UnblockInteraction()
		{
			if (graphicRaycaster)
			{
				graphicRaycaster.enabled = true;
			}
			
			foreach (var current in registeredViews)
			{
				current.Value.AllowInteraction = true;
			}
		}

		private bool IsViewValid(IView view)
		{
			Component viewAsComponent = view as Component;

			if (viewAsComponent == null)
			{
				Debug.LogError($"The View to register must derive from {nameof(Component)}");
				return false;
			}

			if (registeredViews.ContainsKey(view.GetType()))
			{
				Debug.LogError($"{view.GetType().Name} already registered.");
				return false;
			}

			return true;
		}

		private void ProcessViewRegister<TView, TOptions>(TView view, Layer<TView, TOptions> layer)
			where TView : IView
			where TOptions : IViewOptions
		{
			Type viewType = view.GetType();
			Component viewAsComponent = view as Component;
			viewAsComponent.gameObject.SetActive(false);
			layer.RegisterView(view);
			layer.ReparentView(view, viewAsComponent.transform);
			registeredViews.Add(viewType, view);
		}
	}
}
