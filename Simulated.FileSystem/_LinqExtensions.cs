// SimulatableAPI
// File: _LinqExtensions.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;

// ReSharper disable CheckNamespace

namespace System.Linq // ReSharper restore CheckNamespace
{
	internal static class _LinqExtensions
	{
		public static void Each<T>([NotNull] this IEnumerable<T> items, [NotNull] Action<T> op)
		{
			foreach (var item in items)
			{
				op(item);
			}
		}

		public static Task<T> AsImmediateTask<T>(this T result)
		{
			var source = new TaskCompletionSource<T>();
			source.SetResult(result);
			return source.Task;
		}
	}
}
