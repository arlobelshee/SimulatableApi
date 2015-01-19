// SimulatableAPI
// File: _AsyncExtensions.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Simulated._Fs
{
	internal static class _AsyncExtensions
	{
		[NotNull] private static readonly Task<bool> FalseTask = Task.FromResult(false);
		[NotNull] private static readonly Task<bool> TrueTask = Task.FromResult(true);
		[NotNull] public static readonly Task CompletedTask = Task.FromResult(false);

		[NotNull]
		public static Task<T> AsTask<T>([CanBeNull] this T result)
		{
			return Task.FromResult(result);
		}

		[NotNull]
		public static Task<bool> AsTask(this bool result)
		{
			return result ? TrueTask : FalseTask;
		}

		[NotNull]
		public static Task WrapAsNeededForApiUntilIfixTheInsides([NotNull] this Task job)
		{
			return job;
		}

		public static void RunSynchronouslyAsCheapHackUntilIFixScheduling([NotNull] this Task job)
		{
			job.Wait();
		}

		public static bool WaitSynchronouslyUntilIFinishRefactoring([NotNull] this Task<bool> task)
		{
			return task.Result;
		}
	}
}
