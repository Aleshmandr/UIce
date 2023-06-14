﻿namespace Uice
{
	public class BoolToStringOperator : ToOperator<bool, string>
	{
		protected override string Convert(bool value)
		{
			return value.ToString();
		}
	}
}