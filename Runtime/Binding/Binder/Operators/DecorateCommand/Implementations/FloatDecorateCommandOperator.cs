﻿using UnityEngine;

namespace Juice
{
	public class FloatDecorateCommandOperator : DecorateCommandOperator<float>
	{
		protected override ConstantBindingInfo<float> DecorationBindingInfo => decorationBindingInfo;

		[SerializeField] private FloatConstantBindingInfo decorationBindingInfo = new FloatConstantBindingInfo();
	}
}