﻿using System.Collections.Generic;

namespace Maui
{
	public abstract class MergeOperator<T> : BindingListOperatorBinder<T, T>
	{
		protected override T Process(int triggerIndex, IReadOnlyList<T> list)
		{
			return list[triggerIndex];
		}
	}
}