// SimulatableAPI
// File: _TestOperation.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System.Collections.Generic;
using JetBrains.Annotations;
using Simulated._Fs;

namespace Simulated.Tests.zzTestHelpers
{
	internal class _TestOperation : _DiskChange
	{
		private readonly int _name;
		private readonly List<_DiskChange> _enemies = new List<_DiskChange>();

		public _TestOperation(int name)
		{
			_name = name;
		}

		public override bool ConflictsWith(_DiskChange op2)
		{
			return _enemies.Contains(op2);
		}

		public void MakeConflictWith([NotNull] _TestOperation other)
		{
			_enemies.Add(other);
			other._enemies.Add(this);
		}

		public override string ToString()
		{
			return _name.ToString();
		}
	}
}
