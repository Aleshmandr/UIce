﻿using System.Threading.Tasks;
using UnityEngine;

namespace Uice
{
	public interface ITransition
	{
		void Prepare(RectTransform target);
		Task Animate(RectTransform target);
		void Cleanup(RectTransform target);
	}
}