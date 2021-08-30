﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Juice.Utils;

namespace Juice
{
	public class WindowHideLauncher : IWindowHideLauncher
	{
		private readonly Type windowType;
		private readonly Func<WindowHideSettings, Task> hideCallback;
		private readonly Dictionary<string, object> payload;

		private ITransition transition;
		private Type destinationViewType;

		public WindowHideLauncher(Type windowType, Func<WindowHideSettings, Task> hideCallback)
		{
			this.windowType = windowType;
			this.hideCallback = hideCallback;
			payload = new Dictionary<string, object>();
		}

		public IWindowHideLauncher WithTransition(ITransition transition)
		{
			this.transition = transition;
			return this;
		}

		public IWindowHideLauncher WithDestination(Type destinationViewType)
		{
			this.destinationViewType = destinationViewType;
			return this;
		}
		
		public IWindowHideLauncher AddPayload(string key, object value)
		{
			payload[key] = value;
			return this;
		}

		public void Execute()
		{
			ExecuteAsync().RunAndForget();
		}

		public async Task ExecuteAsync()
		{
			await hideCallback(BuildSettings());
		}

		private WindowHideSettings BuildSettings()
		{
			return new WindowHideSettings(windowType, payload, transition, destinationViewType);
		}
	}
}
