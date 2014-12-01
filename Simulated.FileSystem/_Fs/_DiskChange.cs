// SimulatableAPI
// File: _DiskChange.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Simulated._Fs
{
	internal class _DiskChange
	{
		[NotNull] private readonly _DiskChangeKind _kind;
		[NotNull] private readonly Task _work;

		public event Action<_DiskChange> Completed;

		public _DiskChange([NotNull] _DiskChangeKind kind, [NotNull] Task work)
		{
			_kind = kind;
			_work = work;
		}

		[NotNull]
		public _DiskChangeKind Kind
		{
			get { return _kind; }
		}

		public bool ConflictsWith([NotNull] _DiskChange other)
		{
			return Kind.ConflictsWith(other.Kind);
		}

		public void Execute()
		{
			_work.ContinueWith(result =>
			{
				result.ContinueWith(done =>
				{
					if (Completed != null)
						Completed(this);
				});
			});
			_work.Start(TaskScheduler.Default);
		}

		public override string ToString()
		{
			return String.Format("Work({0})", _kind);
		}

		[NotNull]
		public static _DiskChange Make([NotNull] _DiskChangeKind kind, [NotNull] Task<Task> work)
		{
			return new _DiskChange(kind, work);
		}

		[NotNull]
		public static _DiskChange Make<T>([NotNull] _DiskChangeKind kind, [NotNull] Task<Task<T>> work)
		{
			return new _DiskChange(kind, work);
		}
	}
}
