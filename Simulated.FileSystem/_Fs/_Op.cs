// SimulatableAPI
// File: _Op.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System.Linq;
using JetBrains.Annotations;

namespace Simulated._Fs
{
	internal abstract class _Op
	{
		[NotNull] private readonly FsPath _target;
		private static readonly Kind[] _kindsThatConflictWithDirExists = new[] {Kind.DirCreate, Kind.DirDelete, Kind.FileWrite};

		public bool ConflictsWith([NotNull] _Op other)
		{
			var kinds = new[] {OpKind, other.OpKind}.ToList();
			kinds.Sort();
			var higherPriorityKind = kinds[0];
			var otherKind = kinds[1];

			if (higherPriorityKind == Kind.DirDelete)
				return otherKind != Kind.DirDelete;
			if (higherPriorityKind == Kind.DirFindFiles)
				return otherKind == Kind.FileWrite;
			if (higherPriorityKind == Kind.DirExists)
				return _kindsThatConflictWithDirExists.Contains(otherKind);
			if (_target == other._target)
			{
				if (higherPriorityKind == Kind.FileWrite || otherKind == Kind.FileWrite)
					return true;
			}
			return false;
		}

		protected abstract Kind OpKind { get; }

		protected enum Kind
		{
			DirDelete = 1,
			DirFindFiles = 2,
			DirExists = 3,
			DirCreate,
			FileRead,
			FileWrite,
			FileExists,
		}

		protected _Op([NotNull] FsPath target)
		{
			_target = target;
		}

		public override string ToString()
		{
			return string.Format("{0} {1}", GetType()
				.Name, _target);
		}

		[NotNull]
		public static _Op DeleteDirectory([NotNull] FsPath target)
		{
			return new _DirectoryDelete(target);
		}

		[NotNull]
		public static _Op CreateDirectory([NotNull] FsPath target)
		{
			return new _DirectoryCreate(target);
		}

		[NotNull]
		public static _Op WriteFile([NotNull] FsPath target, [NotNull] string contents)
		{
			return new _FileWrite(target, contents);
		}

		[NotNull]
		public static _Op ReadFile([NotNull] FsPath target)
		{
			return new _FileRead(target);
		}

		[NotNull]
		public static _Op FindFiles([NotNull] FsPath target, [NotNull] string pattern)
		{
			return new _DirectoryFindFiles(target, pattern);
		}

		[NotNull]
		public static _Op FileExists([NotNull] FsPath target)
		{
			return new _FileExists(target);
		}

		public static _Op DirectoryExists(FsPath target)
		{
			return new _DirectoryExists(target);
		}
	}
}
