﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Maui
{
	public abstract class Layer<TView, TOptions> : MonoBehaviour
		where TView : IView
		where TOptions : IViewOptions
	{
		protected readonly Dictionary<Type, TView> registeredViews = new Dictionary<Type, TView>();
		
		protected UIFrame uiFrame;
		
		public virtual void Initialize(UIFrame uiFrame)
		{
			this.uiFrame = uiFrame;
		}
		
		public async Task ShowView<TViewModel>(Type viewType, TViewModel viewModel, TOptions overrideOptions) where TViewModel : IViewModel
		{
			if (registeredViews.TryGetValue(viewType, out TView view))
			{
				await ShowView(view, viewModel, overrideOptions);
			}
			else
			{
				Debug.LogError($"View with type {viewType} not registered to this layer!");
			}
		}

		public abstract Task HideView(TView view);

		public async Task HideView(Type viewType)
		{
			if (registeredViews.TryGetValue(viewType, out TView view))
			{
				await HideView(view);
			}
			else
			{
				Debug.LogError($"Could not hide view of type {viewType} as it is not registered to this layer!");
			}
		}

		public virtual Task HideAll(bool animate = true)
		{
			Task[] tasks = new Task[registeredViews.Count];
			int i = 0;

			foreach (KeyValuePair<Type,TView> viewEntry in registeredViews)
			{
				tasks[i] = viewEntry.Value.Hide(animate);
				i++;
			}

			return Task.WhenAll(tasks);
		}

		public virtual void ReparentView(IView view, Transform viewTransform)
		{
			viewTransform.SetParent(transform, false);
		}

		public void RegisterView(TView view)
		{
			Type viewType = view.GetType();

			if (registeredViews.ContainsKey(viewType) == false)
			{
				ProcessViewRegister(view);
			}
			else
			{
				Debug.LogError($"View view already registered for type {viewType}");
			}
		}

		public void UnregisterView(TView view)
		{
			Type viewType = view.GetType();

			if (registeredViews.ContainsKey(viewType))
			{
				ProcessViewUnregister(view);
			}
			else
			{
				Debug.LogError($"View view not registered for type {viewType}");
			}
		}

		public bool IsViewRegistered<T>() where T : TView
		{
			return registeredViews.ContainsKey(typeof(T));
		}
		
		protected abstract Task ShowView<TViewModel>(TView view, TViewModel viewModel, TOptions overrideOptions) where TViewModel : IViewModel;

		protected virtual void ProcessViewRegister(TView view)
		{
			registeredViews.Add(view.GetType(), view);
			view.ViewDestroyed += OnViewDestroyed;
		}

		protected virtual void ProcessViewUnregister(TView view)
		{
			view.ViewDestroyed -= OnViewDestroyed;
			registeredViews.Remove(view.GetType());
		}

		private void OnViewDestroyed(IView view)
		{
			if (registeredViews.ContainsKey(view.GetType()))
			{
				UnregisterView((TView)view);
			}
		}
	}
}
