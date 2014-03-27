// SimulatableAPI
// File: _FileExists.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using JetBrains.Annotations;

namespace Simulated._Fs
{
	internal class _FileExists : _Op
	{
		public _FileExists([NotNull] FsPath target) : base(target) {}

		protected override Kind OpKind
		{
			get { return Kind.FileExists; }
		}
	}
}
