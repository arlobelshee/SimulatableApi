// SimulatableAPI
// File: _DirectoryCreate.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using JetBrains.Annotations;

namespace Simulated._Fs
{
	internal class _DirectoryCreate : _Op
	{
		public _DirectoryCreate([NotNull] FsPath target) : base(target) {}
	}
}
