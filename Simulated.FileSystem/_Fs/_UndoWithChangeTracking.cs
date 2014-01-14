// SimulatableAPI
// File: _UndoWithChangeTracking.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Simulated._Fs
{
	internal class _UndoWithChangeTracking : _StorageTransform
	{
		[NotNull] private readonly List<UndoStep> _stepsTaken = new List<UndoStep>();
		[NotNull] private readonly AsyncLazy<FsPath> _undoDataCache;

		public _UndoWithChangeTracking([NotNull] _StorageSink next) : base(next)
		{
			var cacheLocation = FsPath.TempFolder/("UndoData." + Guid.NewGuid()
				.ToString("N"));
			_undoDataCache = new AsyncLazy<FsPath>(_EnsureDirExists(cacheLocation));
		}

		[NotNull]
		private async Task<FsPath> _EnsureDirExists([NotNull] FsPath cacheLocation)
		{
			if (!await Next.DirExists(cacheLocation))
				await Next.CreateDir(cacheLocation);
			return cacheLocation;
		}

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

		public override Task CreateDir(FsPath path)
		{
			_AllMissingDirectoriesInPathFromBottomUp(path)
				.Reverse()
				.Each(dir => { _AddUndoStep(() => Next.DeleteDir(new FsPath(dir))); });
			return base.CreateDir(path);
		}

		public override async Task DeleteDir(FsPath path)
		{
			var randomDirectoryName = (await _undoDataCache)/Guid.NewGuid()
				.ToString("N");
			await Next.MoveDir(path, randomDirectoryName);
			_AddUndoStep(() => Next.MoveDir(randomDirectoryName, path));
		}

		[NotNull]
		private async Task _Overwrote([NotNull] FsPath path)
		{
			if (!await Next.FileExists(path))
			{
				_AddUndoStep(() => Next.DeleteFile(path));
				return;
			}
			var randomFileName = (await _undoDataCache)/Guid.NewGuid()
				.ToString("N");
			await Next.MoveFile(path, randomFileName);
			_AddUndoStep(() => _RestoreFileFromCache(path, randomFileName));
		}

		public override async Task Overwrite(FsPath path, string newContents)
		{
			await _Overwrote(path);
			await base.Overwrite(path, newContents);
		}

		public override async Task Overwrite(FsPath path, byte[] newContents)
		{
			await _Overwrote(path);
			await base.Overwrite(path, newContents);
		}

		[NotNull]
		private async Task _RestoreFileFromCache([NotNull] FsPath path, [NotNull] FsPath randomFileName)
		{
			await Next.DeleteFile(path);
			await Next.MoveFile(randomFileName, path);
		}

		private void _AddUndoStep([NotNull] Func<Task> undo)
		{
			_stepsTaken.Add(new UndoStep(undo));
		}

		[NotNull]
		private async Task _EnsureUndoDataCacheIsGone()
		{
			var cachePath = await _undoDataCache;
			if (await Next.DirExists(cachePath))
			{
				await Next.DeleteDir(cachePath);
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

		[NotNull]
		public static IEnumerable<string> _AllMissingDirectoriesInPathFromBottomUp([NotNull] FsPath path)
		{
			var dir = new DirectoryInfo(path.Absolute);
			var root = dir.Root;
			while (dir != null && (!dir.Exists && dir != root))
			{
				yield return dir.FullName;
				dir = dir.Parent;
			}
		}
	}
}
