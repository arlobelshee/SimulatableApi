// SimulatableAPI
// File: _OverlappedOperation.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using JetBrains.Annotations;

namespace Simulated._Fs
{
	internal abstract class _DiskChangeKind
	{
		public abstract bool ConflictsWith([NotNull] _DiskChangeKind op2);
	}
}
