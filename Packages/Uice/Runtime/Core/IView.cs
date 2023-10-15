﻿namespace Uice
{
	public delegate void ViewEventHandler(IView controller);

	public interface IView : ITransitionable
	{
		event ViewEventHandler CloseRequested;
		event ViewEventHandler ViewDestroyed;

		bool IsInteractable { get; set; }

		void SetContext(IContext context);
		void Destroy();
	}
}
