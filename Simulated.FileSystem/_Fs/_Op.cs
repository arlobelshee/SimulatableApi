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
		[NotNull]
		public static _OverlappedLambdaWithKind DeleteDirectory([NotNull] FsPath target)
		{
			return new _OverlappedLambdaWithKind(target, _OverlappedLambdaWithKind.Kind.DirDelete);
		}

		[NotNull]
		public static _OverlappedLambdaWithKind CreateDirectory([NotNull] FsPath target)
		{
			return new _OverlappedLambdaWithKind(target, _OverlappedLambdaWithKind.Kind.DirCreate);
		}

		[NotNull]
		public static _OverlappedLambdaWithKind WriteFile([NotNull] FsPath target, [NotNull] string contents)
		{
			return new _OverlappedLambdaWithKind(target, _OverlappedLambdaWithKind.Kind.FileWrite);
		}

		[NotNull]
		public static _OverlappedLambdaWithKind ReadFile([NotNull] FsPath target)
		{
			return new _OverlappedLambdaWithKind(target, _OverlappedLambdaWithKind.Kind.ReadOnlyFileOp);
		}

		[NotNull]
		public static _OverlappedLambdaWithKind FindFiles([NotNull] FsPath target, [NotNull] string pattern)
		{
			return new _OverlappedLambdaWithKind(target, _OverlappedLambdaWithKind.Kind.DirFindFiles);
		}

		[NotNull]
		public static _OverlappedLambdaWithKind FileExists([NotNull] FsPath target)
		{
			return new _OverlappedLambdaWithKind(target, _OverlappedLambdaWithKind.Kind.ReadOnlyFileOp);
		}

		[NotNull]
		public static _OverlappedLambdaWithKind DirectoryExists([NotNull] FsPath target)
		{
			return new _OverlappedLambdaWithKind(target, _OverlappedLambdaWithKind.Kind.DirExists);
		}
	}
}
