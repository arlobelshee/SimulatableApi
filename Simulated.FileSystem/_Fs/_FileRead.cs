// SimulatableAPI
// File: _FileRead.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using JetBrains.Annotations;

namespace Simulated._Fs
{
	internal class _FileRead : _Op
	{
		public _FileRead([NotNull] FsPath target) : base(target) {}
	}
}
