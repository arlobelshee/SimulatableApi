// SimulatableAPI
// File: _Op.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using JetBrains.Annotations;

namespace Simulated._Fs
{
	internal abstract class _Op
	{
		[NotNull] private readonly FsPath _target;

		public bool ConflictsWith([NotNull] _Op other)
		{
			if (this is _DirectoryDelete || other is _DirectoryDelete)
				return !(this is _DirectoryDelete && other is _DirectoryDelete);
			if (_target == other._target)
			{
				if (this is _FileWrite || other is _FileWrite)
					return true;
			}
			return false;
		}

		protected _Op([NotNull] FsPath target)
		{
			_target = target;
		}

		public override string ToString()
		{
			return string.Format("{0} {1}", GetType().Name, _target);
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
	}
}
