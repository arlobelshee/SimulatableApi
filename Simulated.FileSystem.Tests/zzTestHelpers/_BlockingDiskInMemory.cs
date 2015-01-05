// SimulatableAPI
// File: _BlockingDiskInMemory.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Simulated._Fs;

namespace Simulated.Tests.zzTestHelpers
{
	internal class _BlockingDiskInMemory : _IFsDisk
	{
		private const string Wait = "waiting for slice to begin";
		private const string Starting = "starting work on disk and observing its completion";
		private const string Finished = "finished";
		[NotNull] public readonly _IFsDisk Impl = new _DiskSimulated();
		[NotNull] private _BlockedWork _timeslice = new _BlockedWork();
		[NotNull] private readonly List<string> _log = new List<string>();

		public bool DirExistsNeedsToBeMadeDelayStart(FsPath path)
		{
			return Impl.DirExistsNeedsToBeMadeDelayStart(path);
		}

		public async Task<bool> FileExistsNeedsToBeMadeDelayStart(FsPath path)
		{
			const string operation = "check if file exists";
			var myTimeslice = _timeslice;
			_Log(myTimeslice, Wait, operation, path);
			await myTimeslice;
			_Log(myTimeslice, Starting, operation, path);
			var result = await myTimeslice.Executing(Impl.FileExistsNeedsToBeMadeDelayStart(path));
			_Log(myTimeslice, Finished, operation, path);
			return result;
		}

		public async Task<string> TextContentsNeedsToBeMadeDelayStart(FsPath path)
		{
			const string operation = "read text contents";
			var myTimeslice = _timeslice;
			_Log(myTimeslice, Wait, operation, path);
			await myTimeslice;
			_Log(myTimeslice, Starting, operation, path);
			var result = await myTimeslice.Executing(Impl.TextContentsNeedsToBeMadeDelayStart(path));
			_Log(myTimeslice, Finished, operation, path);
			return result;
		}

		public IObservable<byte[]> RawContentsNeedsToBeMadeDelayStart(FsPath path)
		{
			return Impl.RawContentsNeedsToBeMadeDelayStart(path);
		}

		public void CreateDirNeedsToBeMadeDelayStart(FsPath path)
		{
			Impl.CreateDirNeedsToBeMadeDelayStart(path);
		}

		public async Task OverwriteNeedsToBeMadeDelayStart(FsPath path, string newContents)
		{
			const string operation = "write text contents";
			var myTimeslice = _timeslice;
			_Log(myTimeslice, Wait, operation, path);
			await myTimeslice;
			_Log(myTimeslice, Starting, operation, path);
			await myTimeslice.Executing(Impl.OverwriteNeedsToBeMadeDelayStart(path, newContents));
			_Log(myTimeslice, Finished, operation, path);
		}

		public void OverwriteNeedsToBeMadeDelayStart(FsPath path, byte[] newContents)
		{
			Impl.OverwriteNeedsToBeMadeDelayStart(path, newContents);
		}

		public void DeleteDirNeedsToBeMadeDelayStart(FsPath path)
		{
			Impl.DeleteDirNeedsToBeMadeDelayStart(path);
		}

		public void DeleteFileNeedsToBeMadeDelayStart(FsPath path)
		{
			Impl.DeleteFileNeedsToBeMadeDelayStart(path);
		}

		public void MoveFileNeedsToBeMadeDelayStart(FsPath src, FsPath dest)
		{
			Impl.MoveFileNeedsToBeMadeDelayStart(src, dest);
		}

		public void MoveDirNeedsToBeMadeDelayStart(FsPath src, FsPath dest)
		{
			Impl.MoveDirNeedsToBeMadeDelayStart(src, dest);
		}

		public IEnumerable<FsPath> FindFilesNeedsToBeMadeDelayStart(FsPath path, string searchPattern)
		{
			return Impl.FindFilesNeedsToBeMadeDelayStart(path, searchPattern);
		}

		public void ExecuteAllRequestedActionsSynchronously()
		{
			var lastSlice = _timeslice;
			_timeslice = new _BlockedWork();
			_log.Add(String.Format("Time slice {0}: beginning execution.", lastSlice.SequenceNumber));
			lastSlice.ExecuteAll();
			_log.Add(String.Format("Time slice {0}: finished execution.", lastSlice.SequenceNumber));
		}

		private void _Log([NotNull] _BlockedWork when, [NotNull] string action, [NotNull] string operation, [NotNull] FsPath target)
		{
			_log.Add(string.Format("In time slice {0}: {1} while trying to {2} on {3}.", when.SequenceNumber, action, operation, target));
		}

		private class _BlockedWork
		{
			[NotNull] private volatile TaskCompletionSource<bool> _delay = new TaskCompletionSource<bool>();
			[NotNull] private readonly ConcurrentBag<Task> _workInProgress = new ConcurrentBag<Task>();
			private static int _nextSequenceNumber;
			public readonly int SequenceNumber = ++_nextSequenceNumber;

			public TaskAwaiter<bool> GetAwaiter()
			{
				return _delay.Task.GetAwaiter();
			}

			[NotNull]
			public Task Executing([NotNull] Task diskChange)
			{
				_workInProgress.Add(diskChange);
				return diskChange;
			}

			[NotNull]
			public Task<TResult> Executing<TResult>([NotNull] Task<TResult> diskChange)
			{
				_workInProgress.Add(diskChange);
				return diskChange;
			}

			public void ExecuteAll()
			{
				_delay.TrySetResult(true);
				Task.WaitAll(_workInProgress.ToArray());
			}
		}
	}
}
