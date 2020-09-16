﻿using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Maui
{
	[RequireComponent(typeof(ViewModelComponent))]
	public abstract class View<T> : MonoBehaviour, IView, IViewModelInjector
		where T : IViewModel
	{
		public event ViewEventHandler InTransitionFinished;
		public event ViewEventHandler OutTransitionFinished;
		public event ViewEventHandler CloseRequested;
		public event ViewEventHandler ViewDestroyed;

		public bool IsVisible => transitionHandler.IsVisible;
		public IViewModel ViewModel => targetComponent != null ? targetComponent.ViewModel : null;

		public Transition InTransition
		{
			get => inTransition;
			set => inTransition = value;
		}

		public Transition OutTransition
		{
			get => outTransition;
			set => outTransition = value;
		}

		public Type InjectionType => typeof(T);
		public ViewModelComponent Target => targetComponent;

		[Header("Target ViewModel Component")]
		[SerializeField] private ViewModelComponent targetComponent;
		[Header("View Animations")]
		[SerializeField] private Transition inTransition;
		[SerializeField] private Transition outTransition;
		
		private readonly TransitionHandler transitionHandler = new TransitionHandler();

		protected void Reset()
		{
			targetComponent = GetComponentInChildren<ViewModelComponent>();
		}

		public async Task Show(IViewModel viewModel)
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

			OnShowing();

			if (gameObject.activeSelf)
			{
				OnInTransitionFinished();
			}
			else
			{
				await transitionHandler.Show(gameObject, InTransition);
				
				OnInTransitionFinished();
			}
		}

		public async Task Hide(bool animate = true)
		{
			OnHiding();

			await transitionHandler.Hide(gameObject, animate ? outTransition : null);

			SetViewModel(default);
			OnOutTransitionFinished();
		}

		protected virtual void SetViewModel(T viewModel)
		{
			if (targetComponent != null)
			{
				targetComponent.ViewModel = viewModel;
			}
		}

		protected virtual void OnShowing()
		{

		}

		protected virtual void OnInTransitionFinished()
		{
			InTransitionFinished?.Invoke(this);
		}

		protected virtual void OnHiding()
		{

		}

		protected virtual void OnOutTransitionFinished()
		{
			OutTransitionFinished?.Invoke(this);
		}
	}
}
