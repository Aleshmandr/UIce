﻿using UnityEngine;

namespace Juice
{
	public class ColorStartWithOperator : StartWithOperator<Color>
	{
		protected override ConstantBindingInfo<Color> InitialValue => initialValue;

		[SerializeField] private ColorConstantBindingInfo initialValue = new ColorConstantBindingInfo();
	}
}