﻿using System;
using UnityEngine;

namespace Juice
{
	public class TypeConstraintAttribute : PropertyAttribute
	{
		public Type BaseType { get; }
		public bool ForceInstantiableType { get; }

		public TypeConstraintAttribute(Type baseType, bool forceInstantiableType = false)
		{
			BaseType = baseType;
			ForceInstantiableType = forceInstantiableType;
		}
	}
}
