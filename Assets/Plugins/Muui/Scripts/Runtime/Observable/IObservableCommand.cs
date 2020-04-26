﻿namespace Muui
{
	public delegate void ObservableCommandEventHandler<T>(T parameter);
	public delegate void ObservableCommandEventHandler();

	public interface IObservableCommand
	{
		event ObservableCommandEventHandler ExecuteRequested;

		IReadOnlyObservableVariable<bool> CanExecute { get; }

		bool Execute();
	}

	public interface IObservableCommand<T>
	{
		event ObservableCommandEventHandler<T> ExecuteRequested;

		IReadOnlyObservableVariable<bool> CanExecute { get; }

		bool Execute(T parameter);
	}
}
