﻿using Uice.Tweening;

namespace Uice
{
	public class FloatTweenOperator : TweenOperator<float>
	{
		protected override Tweener<float> BuildTweener(Tweener<float>.Getter getter, Tweener<float>.Setter setter, float finalValue, float duration)
		{
			return Tween.To(getter, setter, finalValue, duration);
		}
	}
}