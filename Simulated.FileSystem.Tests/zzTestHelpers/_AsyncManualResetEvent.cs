// SimulatableAPI
// File: _AsyncManualResetEvent.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Simulated.Tests.zzTestHelpers
{
	/// <remarks>
	///    Taken from Stephen Toub's blog: http://blogs.msdn.com/b/pfxteam/archive/2012/02/11/10266920.aspx
	/// </remarks>
	internal class _AsyncManualResetEvent
	{
		[NotNull] private volatile TaskCompletionSource<bool> _impl = new TaskCompletionSource<bool>();

		[NotNull]
		public Task WaitAsync()
		{
			return _impl.Task;
		}

		[NotNull]
		public TaskAwaiter GetAwaiter()
		{
			return WaitAsync()
				.GetAwaiter();
		}

		public void Set()
		{
			_impl.TrySetResult(true);
		}

		public void Reset()
		{
			while (true)
			{
				var tcs = _impl;
#pragma warning disable 420
				if (!tcs.Task.IsCompleted || Interlocked.CompareExchange(ref _impl, new TaskCompletionSource<bool>(), tcs) == tcs)
#pragma warning restore 420
					return;
			}
		}
	}
}
