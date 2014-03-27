// SimulatableAPI
// File: _Op.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System.Linq;
using JetBrains.Annotations;

namespace Simulated._Fs
{
	internal static class _Op
	{
		private static readonly Kind[] KindsThatConflictWithDirExists = {Kind.DirCreate, Kind.DirDelete, Kind.FileWrite};

		public static bool ConflictsWith([NotNull] _OverlappedOperation op1, [NotNull] _OverlappedOperation op2)
		{
			var kinds = new[] {op1.OpKind, op2.OpKind}.ToList();
			kinds.Sort();
			var higherPriorityKind = kinds[0];
			var otherKind = kinds[1];

			if (higherPriorityKind == Kind.DirDelete)
				return otherKind != Kind.DirDelete;
			if (higherPriorityKind == Kind.DirFindFiles)
				return otherKind == Kind.FileWrite;
			if (higherPriorityKind == Kind.DirExists)
				return KindsThatConflictWithDirExists.Contains(otherKind);
			if (op1.HasSameTargetAs(op2))
			{
				if (higherPriorityKind == Kind.FileWrite || otherKind == Kind.FileWrite)
					return true;
			}
			return false;
		}

		public enum Kind
		{
			DirDelete = 1,
			DirFindFiles = 2,
			DirExists = 3,
			DirCreate,
			FileRead,
			FileWrite,
			FileExists,
		}

		[NotNull]
		public static _OverlappedOperation DeleteDirectory([NotNull] FsPath target)
		{
			return new _OverlappedOperation(target, Kind.DirDelete);
		}

		[NotNull]
		public static _OverlappedOperation CreateDirectory([NotNull] FsPath target)
		{
			return new _OverlappedOperation(target, Kind.DirCreate);
		}

		[NotNull]
		public static _OverlappedOperation WriteFile([NotNull] FsPath target, [NotNull] string contents)
		{
			return new _OverlappedOperation(target, Kind.FileWrite);
		}

		[NotNull]
		public static _OverlappedOperation ReadFile([NotNull] FsPath target)
		{
			return new _OverlappedOperation(target, Kind.FileRead);
		}

		[NotNull]
		public static _OverlappedOperation FindFiles([NotNull] FsPath target, [NotNull] string pattern)
		{
			return new _OverlappedOperation(target, Kind.DirFindFiles);
		}

		[NotNull]
		public static _OverlappedOperation FileExists([NotNull] FsPath target)
		{
			return new _OverlappedOperation(target, Kind.FileExists);
		}

		[NotNull]
		public static _OverlappedOperation DirectoryExists([NotNull] FsPath target)
		{
			return new _OverlappedOperation(target, Kind.DirExists);
		}
	}
}
