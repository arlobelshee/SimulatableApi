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
		private readonly FsPath _cachePathUseThePropertyInstead;

		public _UndoWithChangeTracking([NotNull] FileSystem fileSystem)
		{
			_fileSystem = fileSystem;
			_cachePathUseThePropertyInstead = FsPath.TempFolder/("UndoData." + Guid.NewGuid()
				.ToString("N"));
			UndoDataCache = new AsyncLazy<FsPath>(_fileSystem._Disk.DirExists(_cachePathUseThePropertyInstead)
				.ContinueWith(exists =>
				{
					if (!exists.Result)
					{
						_fileSystem._Disk.CreateDir(_cachePathUseThePropertyInstead);
					}
					return _cachePathUseThePropertyInstead;
				}));
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
			_AddUndoStep(() => _fileSystem._Disk.MoveDir(randomDirectoryName, path));
		}

		public override async Task Overwrote(FsPath path)
		{
			if (!_fileSystem._Disk.FileExists(path))
			{
				_AddUndoStep(() => _fileSystem._Disk.DeleteFile(path));
				return;
			}
			var randomFileName = (await UndoDataCache)/Guid.NewGuid()
				.ToString("N");
			_fileSystem._Disk.MoveFile(path, randomFileName);
			_AddUndoStep(() =>
			{
				_fileSystem._Disk.DeleteFile(path);
				_fileSystem._Disk.MoveFile(randomFileName, path);
			});
		}

		private void _AddUndoStep(Action undo)
		{
			_stepsTaken.Add(new UndoStep(undo));
		}

		private async Task _EnsureUndoDataCacheIsGone()
		{
			// Use direct access to the path from here because this is the teardown for the lazy value.
			if (await _fileSystem._Disk.DirExists(_cachePathUseThePropertyInstead))
			{
				_fileSystem._Disk.DeleteDir(_cachePathUseThePropertyInstead);
			}
		}

		public class UndoStep
		{
			public UndoStep([NotNull] Action undo)
			{
				Undo = undo;
			}

			[NotNull]
			public Action Undo { get; private set; }
		}
	}
}
