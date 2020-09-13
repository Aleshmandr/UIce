﻿using Maui.Collections;
using UnityEngine;

namespace Maui
{
	public abstract class MapOperator<TFrom, TTo> : ToOperator<TFrom, TTo>
	{
		protected abstract SerializableDictionary<TFrom, TTo> Mapper { get; }
		protected abstract ConstantBindingInfo Fallback { get; }
		
		private VariableBinding<TTo> fallbackBinding;

		protected override void Awake()
		{
			base.Awake();
			
			fallbackBinding = new VariableBinding<TTo>(Fallback, this);
		}

		protected override void OnEnable()
		{
			fallbackBinding.Bind();
			base.OnEnable();
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			fallbackBinding.Unbind();
		}
		
		protected override TTo Convert(TFrom value)
		{
			if (Mapper.TryGetValue(value, out var result) == false)
			{
				result = fallbackBinding.Property.Value;
			}

			return result;
		}
	}
}