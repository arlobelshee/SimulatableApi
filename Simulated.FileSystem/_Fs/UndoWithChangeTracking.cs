// SimulatableAPI
// File: UndoWithChangeTracking.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Simulated._Fs
{
	internal class _UndoWithChangeTracking : _Undo
	{
		[NotNull] private readonly FileSystem _fileSystem;
		[NotNull] private readonly List<UndoStep> _stepsTaken = new List<UndoStep>();

		public _UndoWithChangeTracking([NotNull] FileSystem fileSystem)
		{
			_fileSystem = fileSystem;
			var cacheLocation = FsPath.TempFolder/("UndoData." + Guid.NewGuid()
				.ToString("N"));
			UndoDataCache = new AsyncLazy<FsPath>(_EnsureDirExists(cacheLocation));
		}

		private async Task<FsPath> _EnsureDirExists(FsPath cacheLocation)
		{
			if (!await _fileSystem._Disk.DirExists(cacheLocation))
				await _fileSystem._Disk.CreateDir(cacheLocation);
			return cacheLocation;
		}

		public AsyncLazy<FsPath> UndoDataCache { get; private set; }

		public override bool IsTrackingChanges
		{
			get { return true; }
		}

		public override Task CommitAll()
		{
			_stepsTaken.Clear();
			return _EnsureUndoDataCacheIsGone();
		}

		public override Task RevertAll()
		{
			Enumerable.Reverse(_stepsTaken)
				.Each(step => step.Undo());
			_stepsTaken.Clear();
			return _EnsureUndoDataCacheIsGone();
		}

		public override void CreatedDirectory(FsPath path)
		{
			_AddUndoStep(() => _fileSystem._Disk.DeleteDir(path));
		}

		public override async Task DeletedDirectory(FsPath path)
		{
			var randomDirectoryName = (await UndoDataCache)/Guid.NewGuid()
				.ToString("N");
			_fileSystem._Disk.MoveDir(path, randomDirectoryName);
			_AddUndoStep(() => { _fileSystem._Disk.MoveDir(randomDirectoryName, path);
				                   return CompletedTask;
			});
		}

		public override async Task Overwrote(FsPath path)
		{
			if (!await _fileSystem._Disk.FileExists(path))
			{
				_AddUndoStep(() => { _fileSystem._Disk.DeleteFile(path);
					                   return CompletedTask;
				});
				return;
			}
			var randomFileName = (await UndoDataCache)/Guid.NewGuid()
				.ToString("N");
			_fileSystem._Disk.MoveFile(path, randomFileName);
			_AddUndoStep(() =>
			{
				_fileSystem._Disk.DeleteFile(path);
				_fileSystem._Disk.MoveFile(randomFileName, path);
				return CompletedTask;
			});
		}

		private void _AddUndoStep(Func<Task> undo)
		{
			_stepsTaken.Add(new UndoStep(undo));
		}

		private async Task _EnsureUndoDataCacheIsGone()
		{
			var cachePath = await UndoDataCache;
			if (await _fileSystem._Disk.DirExists(cachePath))
			{
				await _fileSystem._Disk.DeleteDir(cachePath);
			}
		}

		public class UndoStep
		{
			public UndoStep([NotNull] Func<Task> undo)
			{
				Undo = ()=> undo().Wait();
			}

			[NotNull]
			public Action Undo { get; private set; }
		}
	}
}
