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
		[NotNull]
		public static Task<T> AsTask<T>([CanBeNull] this T result)
		{
			return Task.FromResult(result);
		}

		public static void RunAndWait([NotNull] this Task job)
		{
			job.RunSynchronously();
			job.Wait();
		}

		[NotNull]
		public static Task WrapAsNeededForApiUntilIfixTheInsides([NotNull] this Task job)
		{
			return job;
		}

		public static void RunSynchronouslyAsCheapHackUntilIFixScheduling([NotNull] this Task job)
		{
			job.RunSynchronously();
			job.Wait();
		}

		public static bool TemporaryUnwrapWhileIRefactorIncrementally([NotNull] this Task<bool> fileExists)
		{
			return fileExists.Result;
		}
	}
}
