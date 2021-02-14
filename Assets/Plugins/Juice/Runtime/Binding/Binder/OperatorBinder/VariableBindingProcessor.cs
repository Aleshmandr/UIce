﻿using UnityEngine;

namespace Juice
{
	public abstract class VariableBindingProcessor<TFrom, TTo> : IBindingProcessor
	{
		public IViewModel ViewModel { get; }

		protected readonly VariableBinding<TFrom> variableBinding;
		protected readonly ObservableVariable<TTo> processedVariable;

		protected VariableBindingProcessor(BindingInfo bindingInfo, Component context)
		{
			processedVariable = new ObservableVariable<TTo>();
			ViewModel = new OperatorBinderVariableViewModel<TTo>(processedVariable);
			variableBinding = new VariableBinding<TFrom>(bindingInfo, context);
			variableBinding.Property.Changed += BoundVariableChangedHandler;
		}

		public virtual void Bind()
		{
			variableBinding.Bind();
		}

		public virtual void Unbind()
		{
			variableBinding.Unbind();
		}

		protected abstract TTo ProcessValue(TFrom value);
		
		protected virtual void BoundVariableChangedHandler(TFrom newValue)
		{
			processedVariable.Value = ProcessValue(newValue);
		}
	}
}