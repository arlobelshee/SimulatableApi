// SimulatableAPI
// File: _DirectoryExists.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using JetBrains.Annotations;

namespace Simulated._Fs
{
	internal class _DirectoryExists : _Op
	{
		public _DirectoryExists([NotNull] FsPath target) : base(target) {}

		protected override Kind OpKind
		{
			get { return Kind.DirExists; }
		}
	}
}
