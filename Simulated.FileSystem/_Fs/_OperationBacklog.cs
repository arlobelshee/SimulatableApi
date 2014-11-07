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
		[NotNull] private List<_DiskChangeKind> _pendingWork = new List<_DiskChangeKind>();
		[NotNull] private readonly object _lock = new object();

		public event Action<object, _ParallelSafeWorkSet> WorkIsReadyToExecute;

		public void Enqueue([NotNull] _DiskChangeKind workToDo)
		{
			lock (_lock)
			{
				_pendingWork.Add(workToDo);
			}
		}

		public void FinishedSomeWork([CanBeNull] _DiskChangeKind completedWork)
		{
			var workToDo = new List<_DiskChangeKind>();
			var workToWait = new List<_DiskChangeKind>();
			var processedWork = workToWait.Concat(workToDo);
			lock (_lock)
			{
				foreach (var op in _pendingWork.Where(op => op != completedWork))
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
				_pendingWork = workToWait;
			}
			if (WorkIsReadyToExecute != null && workToDo.Count > 0)
			{
				WorkIsReadyToExecute(this, new _ParallelSafeWorkSet(workToDo));
			}
		}
	}
}
