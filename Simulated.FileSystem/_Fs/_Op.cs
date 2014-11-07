// SimulatableAPI
// File: _Op.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using JetBrains.Annotations;

namespace Simulated._Fs
{
	internal static class _Op
	{
		[NotNull]
		public static _DiskChange DeleteDirectory([NotNull] FsPath target)
		{
			return new _SingleDiskChange(target, _SingleDiskChange.Kind.DirDelete);
		}

		[NotNull]
		public static _DiskChange CreateDirectory([NotNull] FsPath target)
		{
			return new _SingleDiskChange(target, _SingleDiskChange.Kind.DirCreate);
		}

		[NotNull]
		public static _DiskChange WriteFile([NotNull] FsPath target, [NotNull] string contents)
		{
			return new _SingleDiskChange(target, _SingleDiskChange.Kind.FileWrite);
		}

		[NotNull]
		public static _DiskChange ReadFile([NotNull] FsPath target)
		{
			return new _SingleDiskChange(target, _SingleDiskChange.Kind.ReadOnlyFileOp);
		}

		[NotNull]
		public static _DiskChange FindFiles([NotNull] FsPath target, [NotNull] string pattern)
		{
			return new _SingleDiskChange(target, _SingleDiskChange.Kind.DirFindFiles);
		}

		[NotNull]
		public static _DiskChange FileExists([NotNull] FsPath target)
		{
			return new _SingleDiskChange(target, _SingleDiskChange.Kind.ReadOnlyFileOp);
		}

		[NotNull]
		public static _DiskChange DirectoryExists([NotNull] FsPath target)
		{
			return new _SingleDiskChange(target, _SingleDiskChange.Kind.DirExists);
		}

		[NotNull]
		public static _DiskChange MoveDirectory([NotNull] FsPath src, [NotNull] FsPath dest)
		{
			return new _MultipleDiskChanges(new _SingleDiskChange(src, _SingleDiskChange.Kind.DirDelete), new _SingleDiskChange(dest, _SingleDiskChange.Kind.DirCreate));
		}
	}
}
