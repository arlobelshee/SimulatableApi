// SimulatableAPI
// File: _ParallelSafeWorkSet.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Simulated._Fs
{
	internal class _ParallelSafeWorkSet : IEquatable<_ParallelSafeWorkSet>
	{
		[NotNull] private readonly List<_DiskChange> _workToDo;

		public _ParallelSafeWorkSet([NotNull] IEnumerable<_DiskChange> workToDo)
		{
			_workToDo = workToDo.ToList();
		}

		[NotNull]
		public List<_DiskChange> WorkToDo
		{
			get { return _workToDo; }
		}

		public bool Equals([CanBeNull] _ParallelSafeWorkSet other)
		{
			if (ReferenceEquals(null, other))
				return false;
			if (ReferenceEquals(this, other))
				return true;
			return _workToDo.SequenceEqual(other._workToDo);
		}

		public override bool Equals([CanBeNull] object obj)
		{
			return Equals(obj as _ParallelSafeWorkSet);
		}

		public override int GetHashCode()
		{
			return _workToDo.GetHashCode();
		}

		public static bool operator ==(_ParallelSafeWorkSet left, _ParallelSafeWorkSet right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(_ParallelSafeWorkSet left, _ParallelSafeWorkSet right)
		{
			return !Equals(left, right);
		}

		[NotNull]
		public override string ToString()
		{
			return string.Format("Parallel work chunk: {0}", string.Join(", ", _workToDo));
		}
	}
}
