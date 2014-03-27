// SimulatableAPI
// File: _DirectoryFindFiles.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using JetBrains.Annotations;

namespace Simulated._Fs
{
	internal class _DirectoryFindFiles : _Op
	{
		private readonly string _pattern;

		public _DirectoryFindFiles([NotNull] FsPath target, [NotNull] string pattern) : base(target)
		{
			_pattern = pattern;
		}

		protected override Kind OpKind
		{
			get { return Kind.DirFindFiles; }
		}
	}
}
