// SimulatableAPI
// File: _OverlappedOperation.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System;
using System.Linq;
using JetBrains.Annotations;

namespace Simulated._Fs
{
	internal class _SingleDiskChange : _DiskChange
	{
		private readonly FsPath _target;
		private readonly Kind _kind;

		public _SingleDiskChange([NotNull] FsPath target, Kind kind)
		{
			_target = target;
			_kind = kind;
		}

		public Kind OpKind
		{
			get { return _kind; }
		}

		public bool HasSameTargetAs([NotNull] _SingleDiskChange op2)
		{
			return _target == op2._target;
		}

		public override string ToString()
		{
			return String.Format("{0} {1}", GetType()
				.Name, _target);
		}

		public override bool ConflictsWith(_DiskChange op2)
		{
			var other = op2 as _SingleDiskChange;
			if (other == null)
				return false;
			var kinds = new[] {OpKind, other.OpKind}.ToList();
			kinds.Sort();
			var higherPriorityKind = kinds[0];
			var otherKind = kinds[1];

			if (higherPriorityKind == Kind.DirDelete)
				return otherKind != Kind.DirDelete;
			if (higherPriorityKind == Kind.DirFindFiles)
				return otherKind == Kind.FileWrite;
			if (higherPriorityKind == Kind.DirExists)
				return KindsThatConflictWithDirExists.Contains(otherKind);
			if (HasSameTargetAs(other))
			{
				if (higherPriorityKind == Kind.FileWrite || otherKind == Kind.FileWrite)
					return true;
			}
			return false;
		}

		private static readonly Kind[] KindsThatConflictWithDirExists = {Kind.DirCreate, Kind.DirDelete, Kind.FileWrite};

		public enum Kind
		{
			DirDelete = 1,
			DirFindFiles = 2,
			DirExists = 3,
			DirCreate,
			FileWrite,
			ReadOnlyFileOp
		}
	}
}
