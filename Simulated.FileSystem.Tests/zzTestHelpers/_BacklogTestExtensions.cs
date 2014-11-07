// SimulatableAPI
// File: BacklogTestHelpers.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System.Linq;
using JetBrains.Annotations;
using Simulated._Fs;

namespace Simulated.Tests.zzTestHelpers
{
	internal static class _BacklogTestExtensions
	{
		public static void EnqueueAll([NotNull] this _OperationBacklog testSubject, [NotNull] _DiskChange[] ops)
		{
			ops.Each(testSubject.Enqueue);
		}

		public static void CreateConflict([NotNull] this _DiskChange[] work, int first, int second)
		{
			((_TestOperation) work[first].Kind).MakeConflictWith((_TestOperation) work[second].Kind);
		}
	}
}
