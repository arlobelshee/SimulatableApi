// SimulatableAPI
// File: _OperationBacklog.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System.Collections.Concurrent;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Simulated._Fs
{
	internal class _OperationBacklog
	{
		private readonly ConcurrentQueue<_OverlappedOperation> _pendingWork = new ConcurrentQueue<_OverlappedOperation>();
		private readonly object _lock = new object();

		public void Enqueue([NotNull] _OverlappedOperation workToDo)
		{
			_pendingWork.Enqueue(workToDo);
		}

		[NotNull]
		public List<_OverlappedOperation> DequeueSchedulableWork([NotNull] _OverlappedOperation[] operationsCurrentlyExecuting)
		{
			var workToDo = new List<_OverlappedOperation>();
			_OverlappedOperation nextItem;
			while (_pendingWork.TryPeek(out nextItem))
			{
				if (!_pendingWork.TryDequeue(out nextItem))
					break;
				workToDo.Add(nextItem);
			}
			return workToDo;
		}
	}
}
