// SimulatableAPI
// File: _OperationBacklog.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Simulated._Fs
{
	internal class _OperationBacklog
	{
		private List<_OverlappedOperation> _pendingWork = new List<_OverlappedOperation>();
		private readonly object _lock = new object();

		public void Enqueue([NotNull] _OverlappedOperation workToDo)
		{
			lock (_lock)
			{
				_pendingWork.Add(workToDo);
			}
		}

		[NotNull]
		public List<_OverlappedOperation> FinishedSomeWorkWhatShouldIDoNext([CanBeNull] _OverlappedOperation completedWork)
		{
			var workToDo = new List<_OverlappedOperation>();
			var workToWait = new List<_OverlappedOperation>();
			var processedWork = workToWait.Concat(workToDo);
			lock (_lock)
			{
				foreach (var op in _pendingWork)
				{
					if (op == completedWork)
						continue;

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
			return workToDo;
		}
	}
}
