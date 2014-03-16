// SimulatableAPI
// File: _ObservableContext.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Simulated._Fs
{
	internal class _ObservableContext<T>
	{
		[NotNull] private readonly IObserver<T> _observer;
		private readonly CancellationTokenSource _cancel;

		public _ObservableContext([NotNull] IObserver<T> observer)
		{
			_observer = observer;
			_cancel = new CancellationTokenSource();
		}

		public bool IsCancelled
		{
			get { return _cancel.IsCancellationRequested; }
		}

		public CancellationToken CancelToken
		{
			get { return _cancel.Token; }
		}

		public void Cancel()
		{
			_cancel.Cancel();
		}

		public void OnNext(T value)
		{
			_observer.OnNext(value);
		}

		public void RunUntilCompleteOrError([NotNull] Func<_ObservableContext<T>, Task> execInner)
		{
			try
			{
				execInner(this)
					.Wait();
			}
			catch (Exception ex)
			{
				_observer.OnError(ex);
			}
			finally
			{
				_observer.OnCompleted();
			}
		}

		public void RunUntilCompleteOrError([NotNull] Action<_ObservableContext<T>> execInner)
		{
			try
			{
				execInner(this);
			}
			catch (Exception ex)
			{
				_observer.OnError(ex);
			}
			finally
			{
				_observer.OnCompleted();
			}
		}
	}
}
