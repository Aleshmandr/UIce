﻿using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Uice
{
	public class WaitTransition : ComponentTransition
	{
		[SerializeField] private ConstantBindingInfo<float> time = new ConstantBindingInfo<float>();

		private VariableBinding<float> timeBinding;

		protected override void OnDisable()
		{
			base.OnDisable();
			
			timeBinding?.Unbind();
		}

		protected override void PrepareInternal(Transform target)
		{
			EnsureBinding();
			timeBinding.Bind();
		}

		protected override async Task AnimateInternal(Transform target)
		{
			var initialAnimationTime = DateTime.UtcNow;

			while ((DateTime.UtcNow - initialAnimationTime).TotalSeconds < timeBinding.Property.GetValue(0f))
			{
				await Task.Yield();
			}
		}

		protected override void CleanupInternal(Transform target)
		{
			timeBinding.Unbind();
		}

		private void EnsureBinding()
		{
			timeBinding ??= new VariableBinding<float>(time, this);
		}
	}
}
