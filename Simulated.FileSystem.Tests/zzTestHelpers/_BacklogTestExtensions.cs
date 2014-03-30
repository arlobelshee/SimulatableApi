// SimulatableAPI
// File: BacklogTestHelpers.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using JetBrains.Annotations;
using Simulated._Fs;

namespace Simulated.Tests.zzTestHelpers
{
	internal static class _BacklogTestExtensions
	{
		public static void EnqueueAll([NotNull] this _OperationBacklog testSubject, [NotNull] _TestOperation[] ops)
		{
			foreach (var op in ops)
			{
				testSubject.Enqueue(op);
			}
		}

		public static void CreateConflict([NotNull] this _TestOperation[] work, int first, int second)
		{
			work[first].MakeConflictWith(work[second]);
		}
	}
}
