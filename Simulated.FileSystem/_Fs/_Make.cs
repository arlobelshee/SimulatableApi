// SimulatableAPI
// File: _Make.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Simulated._Fs
{
	internal static class _Make
	{
		[NotNull]
		public static IObservable<T> Observable<T>([NotNull] Func<_ObservableContext<T>, Task> execInner)
		{
			return System.Reactive.Linq.Observable.Create<T>(obs =>
			{
				var context = new _ObservableContext<T>(obs);
				context.RunUntilCompleteOrError(execInner);
				return context.Cancel;
			});
		}

		[NotNull]
		public static IObservable<T> Observable<T>([NotNull] Action<_ObservableContext<T>> execInner)
		{
			return System.Reactive.Linq.Observable.Create<T>(obs =>
			{
				var context = new _ObservableContext<T>(obs);
				context.RunUntilCompleteOrError(execInner);
				return context.Cancel;
			});
		}
	}
}
