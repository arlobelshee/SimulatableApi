// SimulatableAPI
// File: _OverlappedOperation.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using JetBrains.Annotations;

namespace Simulated._Fs
{
	internal class _OverlappedOperation
	{
		private readonly FsPath _target;
		private readonly _Op.Kind _kind;

		public _OverlappedOperation([NotNull] FsPath target, _Op.Kind kind)
		{
			_target = target;
			_kind = kind;
		}

		public _Op.Kind OpKind
		{
			get { return _kind; }
		}

		public bool HasSameTargetAs([NotNull] _OverlappedOperation op2)
		{
			return _target == op2._target;
		}

		public override string ToString()
		{
			return string.Format("{0} {1}", GetType()
				.Name, _target);
		}
	}
}
