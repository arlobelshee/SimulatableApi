// SimulatableAPI
// File: AsyncLazy.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System;
using System.Threading.Tasks;
using Microsoft.Runtime.CompilerServices;

namespace Simulated._Fs
{
	public class AsyncLazy<T> : Lazy<Task<T>>
	{
		public AsyncLazy(Func<T> valueFactory) : base(() => Task.Factory.StartNew(valueFactory)) {}
		public AsyncLazy(Task<T> valueFactory) : base(() => valueFactory) {}

		public AsyncLazy(Func<Task<T>> taskFactory) : base(() => Task.Factory.StartNew(taskFactory)
			.Unwrap()) {}

		public TaskAwaiter<T> GetAwaiter()
		{
			return Value.GetAwaiter();
		}
	}
}
