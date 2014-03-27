// SimulatableAPI
// File: _FileWrite.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using JetBrains.Annotations;

namespace Simulated._Fs
{
	internal class _FileWrite : _Op
	{
		private readonly string _contents;

		public _FileWrite([NotNull] FsPath target, [NotNull] string contents) : base(target)
		{
			_contents = contents;
		}

		protected override Kind OpKind
		{
			get { return Kind.FileWrite; }
		}
	}
}
