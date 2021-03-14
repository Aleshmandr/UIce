﻿using UnityEngine;

namespace Juice
{
	[RequireComponent(typeof(Widget))]
	public class WidgetBinder : VariableBinder<bool>
	{
		protected override string BindingInfoName { get; } = "Show";

		private Widget widget;

		protected override void Awake()
		{
			base.Awake();

			widget = GetComponent<Widget>();
		}

		protected override void Refresh(bool value)
		{
			if (value)
			{
				widget.Show().RunAndForget();
			}
			else
			{
				widget.Hide().RunAndForget();
			}
		}
	}
}
