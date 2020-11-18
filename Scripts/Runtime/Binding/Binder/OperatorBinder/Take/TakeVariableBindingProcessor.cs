﻿

using UnityEngine;

namespace Juice
{
	public class TakeVariableBindingProcessor<T> : VariableBindingProcessor<T, T>
	{
		private readonly int takeAmount;
		private int valueReceivedCount;

		public TakeVariableBindingProcessor(BindingInfo bindingInfo, Component context, int takeAmount)
		: base (bindingInfo, context)
		{
			this.takeAmount = takeAmount;
			valueReceivedCount = 0;
		}

		public override void Bind()
		{
			valueReceivedCount = 0;
			
			base.Bind();
		}

		protected override void BoundVariableChangedHandler(T newValue)
		{
			valueReceivedCount++;
			
			if (valueReceivedCount <= takeAmount)
			{
				base.BoundVariableChangedHandler(newValue);
			}
		}
		
		protected override T ProcessValue(T value)
		{
			return value;
		}
	}
}