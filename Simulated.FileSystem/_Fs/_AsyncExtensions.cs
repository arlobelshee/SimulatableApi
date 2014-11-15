// SimulatableAPI
// File: _AsyncExtensions.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Simulated._Fs
{
	internal static class _AsyncExtensions
	{
		[NotNull]
		public static Task<T> AsTask<T>([CanBeNull] this T result)
		{
			return Task.FromResult(result);
		}
	}
}
