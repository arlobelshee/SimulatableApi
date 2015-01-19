// SimulatableAPI
// File: _DelayingDisk.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Simulated._Fs
{
	internal class _DelayingDisk
	{
		[NotNull] private readonly _IFsDisk _storage;
		[NotNull] private readonly _OperationBacklog _workSchedule;

		public _DelayingDisk([NotNull] _IFsDisk storage)
		{
			_storage = storage;
			_workSchedule = new _OperationBacklog();
			_workSchedule.WorkIsReadyToExecute += (_, workToDo) => { workToDo.StartAll(); };
			_workSchedule.Start();
		}

		[NotNull]
		public Task<bool> FileExists([NotNull] FsPath location)
		{
			var work = new Task<Task<bool>>(() => _storage.FileExistsNeedsToBeMadeDelayStart(location));
			_Schedule(_DiskChange.Make(_Op.FileExists(location), work));
			return work.Unwrap();
		}

		public void Overwrite([NotNull] FsPath location, [NotNull] string contents)
		{
			var work = _storage.Overwrite(location, contents);
			_Schedule(_DiskChange.Make(_Op.WriteFile(location), work));
		}

		private void _Schedule([NotNull] _DiskChange workToDo)
		{
			workToDo.Completed += _workSchedule.FinishedSomeWork;
			_workSchedule.Enqueue(workToDo);
		}
	}
}
