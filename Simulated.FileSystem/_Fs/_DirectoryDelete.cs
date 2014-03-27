// SimulatableAPI
// File: _DirectoryDelete.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using JetBrains.Annotations;

namespace Simulated._Fs
{
	internal class _DirectoryDelete : _Op
	{
		public _DirectoryDelete([NotNull] FsPath target) : base(target) {}

		protected override Kind OpKind
		{
			get { return Kind.DirDelete; }
		}
	}
}
