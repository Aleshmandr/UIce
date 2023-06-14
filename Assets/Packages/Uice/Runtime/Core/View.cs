﻿using System;
using UnityEngine;

namespace Juice
{
	[RequireComponent(typeof(RectTransform))]
	[RequireComponent(typeof(ViewModelComponent))]
	[RequireComponent(typeof(InteractionBlockingTracker))]
	public abstract class View<T> : Widget, IView, IViewModelInjector, IViewModelProvider<T> where T : IViewModel
	{
		public event ViewEventHandler CloseRequested;
		public event ViewEventHandler ViewDestroyed;
		public event ViewModelChangeEventHandler<T> ViewModelChanged;

		public bool IsInteractable
		{
			get
			{
				EnsureInitialState();
				return blockingTracker.IsInteractable;
			}

			set
			{
				EnsureInitialState();
				blockingTracker.IsInteractable = value;
			}
		}

		public Type InjectionType => typeof(T);
		public ViewModelComponent Target => targetComponent;
		public T ViewModel => (T)(Target ? Target.ViewModel : default);

		[SerializeField, HideInInspector] private ViewModelComponent targetComponent;
		[SerializeField, HideInInspector] private InteractionBlockingTracker blockingTracker;

		protected virtual void Reset()
		{
			RetrieveRequiredComponents();
		}

		protected virtual void OnDestroy()
		{
			ViewDestroyed?.Invoke(this);
		}

		public virtual void SetViewModel(IViewModel viewModel)
		{
			if (viewModel != null)
			{
				if (viewModel is T typedViewModel)
				{
					SetViewModel(typedViewModel);
				}
				else
				{
					Debug.LogError($"ViewModel passed have wrong type! ({viewModel.GetType()} instead of {typeof(T)})", this);
				}
			}
		}

		public void Close()
		{
			CloseRequested?.Invoke(this);
		}

		protected override void Initialize()
		{
			base.Initialize();

			RetrieveRequiredComponents();

			Target.ViewModelChanged += OnTargetComponentViewModelChanged;
		}

		protected virtual void SetViewModel(T viewModel)
		{
			EnsureInitialState();

			if (targetComponent)
			{
				targetComponent.ViewModel = viewModel;
			}
		}

		protected virtual void OnViewModelChanged(T lastViewModel, T newViewModel)
		{
			ViewModelChanged?.Invoke(this, lastViewModel, newViewModel);
		}

		private void RetrieveRequiredComponents()
		{
			if (!targetComponent)
			{
				targetComponent = GetComponent<ViewModelComponent>();
			}

			if (!blockingTracker)
			{
				blockingTracker = GetComponent<InteractionBlockingTracker>();
			}
		}

		private void OnTargetComponentViewModelChanged(IViewModelProvider<IViewModel> source, IViewModel lastViewModel, IViewModel newViewModel)
		{
			OnViewModelChanged((T)lastViewModel, (T)newViewModel);
		}
	}
}
