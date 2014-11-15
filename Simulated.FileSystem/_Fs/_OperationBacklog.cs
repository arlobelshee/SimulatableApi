// SimulatableAPI
// File: _OperationBacklog.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Simulated._Fs
{
	internal class _OperationBacklog
	{
		[NotNull] private readonly List<_DiskChange> _pendingWork = new List<_DiskChange>();
		[NotNull] private readonly List<_DiskChange> _workInProgress = new List<_DiskChange>();
		[NotNull] private readonly object _lock = new object();
		private bool _started;

		public event Action<object, _ParallelSafeWorkSet> WorkIsReadyToExecute;

		public void Enqueue([NotNull] _DiskChange workToDo)
		{
			lock (_lock)
			{
				_pendingWork.Add(workToDo);
			}
			if (_started)
// ReSharper disable once AssignNullToNotNullAttribute
				FinishedSomeWork(null);
		}

		public void Start()
		{
			_started = true;
// ReSharper disable once AssignNullToNotNullAttribute
			FinishedSomeWork(null);
		}

		public void FinishedSomeWork([NotNull] _DiskChange completedWork)
		{
			var workToDo = new List<_DiskChange>();
			var workToWait = new List<_DiskChange>();
			var processedWork = _workInProgress.Concat(workToWait)
				.Concat(workToDo);
			lock (_lock)
			{
				_workInProgress.Remove(completedWork);
				foreach (var op in _pendingWork)
				{
// ReSharper disable once PossibleMultipleEnumeration
					if (processedWork.Any(w => w.ConflictsWith(op)))
					{
						workToWait.Add(op);
					}
					else
					{
						workToDo.Add(op);
					}
				}
				_workInProgress.AddRange(workToDo);
				workToDo.Each(op => _pendingWork.Remove(op));
			}
			if (WorkIsReadyToExecute != null && workToDo.Count > 0)
			{
				WorkIsReadyToExecute(this, new _ParallelSafeWorkSet(workToDo));
			}
		}
	}
}
