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

		public bool DirExists(FsPath path)
		{
			return Impl.DirExists(path);
		}

		public Task<bool> FileExists(FsPath path)
		{
			const string operation = "check if file exists";
			return _ExecuteWhenTimesliceOpens(path, operation, () => Impl.FileExists(path));
		}

		public Task<string> TextContents(FsPath path)
		{
			const string operation = "read text contents";
			return _ExecuteWhenTimesliceOpens(path, operation, () => Impl.TextContents(path));
		}

		public Task Overwrite(FsPath path, string newContents)
		{
			const string operation = "write text contents";
			return _ExecuteWhenTimesliceOpens(path, operation, () => Impl.Overwrite(path, newContents));
		}

		public Task Overwrite(FsPath path, byte[] newContents)
		{
			const string operation = "write binary contents";
			return _ExecuteWhenTimesliceOpens(path, operation, () => Impl.Overwrite(path, newContents));
		}

		public IObservable<byte[]> RawContents(FsPath path)
		{
			return Impl.RawContents(path);
		}

		public Task CreateDir(FsPath path)
		{
			return Impl.CreateDir(path);
		}

		public Task DeleteDir(FsPath path)
		{
			return Impl.DeleteDir(path);
		}

		public void DeleteFile(FsPath path)
		{
			Impl.DeleteFile(path);
		}

		public void MoveFile(FsPath src, FsPath dest)
		{
			Impl.MoveFile(src, dest);
		}

		public void MoveDir(FsPath src, FsPath dest)
		{
			Impl.MoveDir(src, dest);
		}

		public IEnumerable<FsPath> FindFiles(FsPath path, string searchPattern)
		{
			return Impl.FindFiles(path, searchPattern);
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

		[NotNull]
		private async Task _ExecuteWhenTimesliceOpens([NotNull] FsPath path, [NotNull] string operation, [NotNull] Func<Task> work)
		{
			var myTimeslice = _timeslice;
			_Log(myTimeslice, Wait, operation, path);
			await myTimeslice;
			_Log(myTimeslice, Starting, operation, path);
			await myTimeslice.Executing(work());
			_Log(myTimeslice, Finished, operation, path);
		}

		[NotNull]
		private async Task<T> _ExecuteWhenTimesliceOpens<T>([NotNull] FsPath path, [NotNull] string operation, [NotNull] Func<Task<T>> work)
		{
			var myTimeslice = _timeslice;
			_Log(myTimeslice, Wait, operation, path);
			await myTimeslice;
			_Log(myTimeslice, Starting, operation, path);
			var result = await myTimeslice.Executing(work());
			_Log(myTimeslice, Finished, operation, path);
			return result;
		}
	}
}
