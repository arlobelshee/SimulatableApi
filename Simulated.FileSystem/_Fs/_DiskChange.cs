// SimulatableAPI
// File: _DiskChange.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using JetBrains.Annotations;

namespace Simulated._Fs
{
	internal class _DiskChange
	{
		[NotNull] private readonly _DiskChangeKind _kind;

		public _DiskChange([NotNull] _DiskChangeKind kind)
		{
			_kind = kind;
		}

		[NotNull]
		public _DiskChangeKind Kind
		{
			get { return _kind; }
		}

		public bool ConflictsWith([NotNull] _DiskChange other)
		{
			return Kind.ConflictsWith(other.Kind);
		}
	}
}
