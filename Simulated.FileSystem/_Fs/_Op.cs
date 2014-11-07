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
		public static _DiskChangeKind DeleteDirectory([NotNull] FsPath target)
		{
			return new _SingleDiskChange(target, _SingleDiskChange.Kind.DirDelete);
		}

		[NotNull]
		public static _DiskChangeKind CreateDirectory([NotNull] FsPath target)
		{
			return new _SingleDiskChange(target, _SingleDiskChange.Kind.DirCreate);
		}

		[NotNull]
		public static _DiskChangeKind WriteFile([NotNull] FsPath target)
		{
			return new _SingleDiskChange(target, _SingleDiskChange.Kind.FileWrite);
		}

		[NotNull]
		public static _DiskChangeKind DeleteFile([NotNull] FsPath target)
		{
			return new _SingleDiskChange(target, _SingleDiskChange.Kind.FileWrite);
		}

		[NotNull]
		public static _DiskChangeKind ReadFile([NotNull] FsPath target)
		{
			return new _SingleDiskChange(target, _SingleDiskChange.Kind.ReadOnlyFileOp);
		}

		[NotNull]
		public static _DiskChangeKind FindFiles([NotNull] FsPath target)
		{
			return new _SingleDiskChange(target, _SingleDiskChange.Kind.DirFindFiles);
		}

		[NotNull]
		public static _DiskChangeKind FileExists([NotNull] FsPath target)
		{
			return new _SingleDiskChange(target, _SingleDiskChange.Kind.ReadOnlyFileOp);
		}

		[NotNull]
		public static _DiskChangeKind DirectoryExists([NotNull] FsPath target)
		{
			return new _SingleDiskChange(target, _SingleDiskChange.Kind.DirExists);
		}

		[NotNull]
		public static _DiskChangeKind MoveDirectory([NotNull] FsPath src, [NotNull] FsPath dest)
		{
			return new _MultipleDiskChanges(DeleteDirectory(src), CreateDirectory(dest));
		}

		[NotNull]
		public static _DiskChangeKind MoveFile([NotNull] FsPath src, [NotNull] FsPath dest)
		{
			return new _MultipleDiskChanges(DeleteFile(src), WriteFile(dest));
		}
	}
}
