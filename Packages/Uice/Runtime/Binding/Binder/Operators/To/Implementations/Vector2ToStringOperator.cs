﻿using UnityEngine;

namespace Uice
{
	public class Vector2ToStringOperator : ToOperator<Vector2, string>
	{
		protected override string Convert(Vector2 value)
		{
			return value.ToString();
		}
	}
}