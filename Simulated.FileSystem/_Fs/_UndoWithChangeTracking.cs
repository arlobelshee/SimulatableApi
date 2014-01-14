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
		[NotNull] private readonly List<UndoStep> _stepsTaken = new List<UndoStep>();
		[NotNull] private readonly _IFsDisk _disk;

		public _UndoWithChangeTracking([NotNull] _IFsDisk disk)
		{
			_disk = disk;
			var cacheLocation = FsPath.TempFolder/("UndoData." + Guid.NewGuid()
				.ToString("N"));
			UndoDataCache = new AsyncLazy<FsPath>(_EnsureDirExists(cacheLocation));
		}

		[NotNull]
		private async Task<FsPath> _EnsureDirExists([NotNull] FsPath cacheLocation)
		{
			if (!await _disk.DirExists(cacheLocation))
				await _disk.CreateDir(cacheLocation);
			return cacheLocation;
		}

		[NotNull]
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
			_AddUndoStep(() => _disk.DeleteDir(path));
		}

		public override async Task DeletedDirectory(FsPath path)
		{
			var randomDirectoryName = (await UndoDataCache)/Guid.NewGuid()
				.ToString("N");
			await _disk.MoveDir(path, randomDirectoryName);
			_AddUndoStep(() => _disk.MoveDir(randomDirectoryName, path));
		}

		public override async Task Overwrote(FsPath path)
		{
			if (!await _disk.FileExists(path))
			{
				_AddUndoStep(() => _disk.DeleteFile(path));
				return;
			}
			var randomFileName = (await UndoDataCache)/Guid.NewGuid()
				.ToString("N");
			await _disk.MoveFile(path, randomFileName);
			_AddUndoStep(() => _RestoreFileFromCache(path, randomFileName));
		}

		[NotNull]
		private async Task _RestoreFileFromCache([NotNull] FsPath path, [NotNull] FsPath randomFileName)
		{
			await _disk.DeleteFile(path);
			await _disk.MoveFile(randomFileName, path);
		}

		private void _AddUndoStep([NotNull] Func<Task> undo)
		{
			_stepsTaken.Add(new UndoStep(undo));
		}

		[NotNull]
		private async Task _EnsureUndoDataCacheIsGone()
		{
			var cachePath = await UndoDataCache;
			if (await _disk.DirExists(cachePath))
			{
				await _disk.DeleteDir(cachePath);
			}
		}

		public class UndoStep
		{
			public UndoStep([NotNull] Func<Task> undo)
			{
				Undo = () => undo()
					.Wait();
			}

			[NotNull]
			public Action Undo { get; private set; }
		}
	}
}
