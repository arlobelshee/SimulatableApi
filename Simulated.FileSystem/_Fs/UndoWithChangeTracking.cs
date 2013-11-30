﻿using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Simulated._Fs
{
	internal class _UndoWithChangeTracking : _Undo
	{
		[NotNull] private readonly FileSystem _fileSystem;
		[NotNull] private readonly List<UndoStep> _stepsTaken = new List<UndoStep>();

		public _UndoWithChangeTracking(FileSystem fileSystem)
		{
			_fileSystem = fileSystem;
		}

		public override bool IsTrackingChanges
		{
			get { return true; }
		}

		public override void CommitAll()
		{
			_stepsTaken.Clear();
			_EnsureUndoDataCacheIsGone();
		}

		public override void RevertAll()
		{
			Enumerable.Reverse(_stepsTaken).Each(step => step.Undo());
			_stepsTaken.Clear();
			_EnsureUndoDataCacheIsGone();
		}

		public override void CreatedDirectory(FsPath path)
		{
			_AddUndoStep(() => _fileSystem._Disk.DeleteDir(path));
		}

		public override void DeletedDirectory(FsPath path)
		{
			_EnsureUndoDataCacheExists();
			var randomDirectoryName = UndoDataCache / Guid.NewGuid().ToString("N");
			_fileSystem._Disk.MoveDir(path, randomDirectoryName);
			_AddUndoStep(() => _fileSystem._Disk.MoveDir(randomDirectoryName, path));
		}

		public override void Overwrote(FsPath path)
		{
			if (!_fileSystem._Disk.FileExists(path))
			{
				_AddUndoStep(() => _fileSystem._Disk.DeleteFile(path));
				return;
			}
			_EnsureUndoDataCacheExists();
			var randomFileName = UndoDataCache/Guid.NewGuid().ToString("N");
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

		private void _EnsureUndoDataCacheExists()
		{
			if (!_fileSystem._Disk.DirExists(UndoDataCache))
			{
				_fileSystem._Disk.CreateDir(UndoDataCache);
			}
		}

		private void _EnsureUndoDataCacheIsGone()
		{
			if (_fileSystem._Disk.DirExists(UndoDataCache))
			{
				_fileSystem._Disk.DeleteDir(UndoDataCache);
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
