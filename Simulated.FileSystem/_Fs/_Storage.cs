// SimulatableAPI
// File: Storage.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Simulated._Fs
{
	internal class _Storage
	{
		[NotNull] private readonly FileSystem _allFiles;
		[NotNull] private readonly _StorageSink _disk;
		[NotNull] private _StorageSink _changes;

		public _Storage([NotNull] FileSystem allFiles, [NotNull] _StorageSink disk)
		{
			_allFiles = allFiles;
			_disk = disk;
			_changes = disk;
		}

		[NotNull]
		public Task<bool> IsDirectory([NotNull] FsPath path)
		{
			return _disk.DirExists(path);
		}

		[NotNull]
		public async Task EnsureDirectoryExists([NotNull] FsPath path)
		{
			if (await IsDirectory(path))
				return;
			_AllMissingDirectoriesInPathFromBottomUp(path)
				.Reverse()
				.Each(dir => _changes.CreatedDirectory(new FsPath(dir)));
			await _disk.CreateDir(path);
		}

		[NotNull]
		public async Task EnsureDirectoryDoesNotExist([NotNull] FsPath path)
		{
			if (!await IsDirectory(path))
				return;
			await _changes.DeletedDirectory(path);
			if (await IsDirectory(path))
				await _disk.DeleteDir(path);
		}

		[NotNull]
		public async Task<IEnumerable<FsFile>> KnownFilesIn([NotNull] string searchPattern, [NotNull] FsPath path)
		{
			return (await _disk.FindFiles(path, searchPattern)).Select(p => new FsFile(_allFiles, p, this));
		}

		[NotNull]
		private IEnumerable<string> _AllMissingDirectoriesInPathFromBottomUp([NotNull] FsPath path)
		{
			var dir = new DirectoryInfo(path.Absolute);
			var root = dir.Root;
			while (dir != null && (!dir.Exists && dir != root))
			{
				yield return dir.FullName;
				dir = dir.Parent;
			}
		}

		[NotNull]
		public Task<bool> IsFile([NotNull] FsPath path)
		{
			return _disk.FileExists(path);
		}

		[NotNull]
		public async Task OverwriteFileContents([NotNull] FsPath path, [NotNull] string newContents, [NotNull] FsDirectory parent)
		{
			await parent.EnsureExists();
			await _changes.Overwrite(path, newContents);
		}

		[NotNull]
		public async Task OverwriteFileContentsBinary([NotNull] FsPath path, [NotNull] byte[] newContents, [NotNull] FsDirectory parent)
		{
			await parent.EnsureExists();
			await _changes.Overwrite(path, newContents);
		}

		[NotNull]
		public Task<string> TextContents([NotNull] FsPath path)
		{
			return _disk.TextContents(path);
		}

		[NotNull]
		public Task<byte[]> RawContents([NotNull] FsPath path)
		{
			return _disk.RawContents(path);
		}

		[NotNull]
		public Task RevertChanges()
		{
			if (_changes.IsTrackingChanges)
			{
				var oldChanges = _changes;
				_changes = oldChanges.Next;
				return oldChanges.RevertAll();
			}
			return _StorageSink.CompletedTask;
		}

		[NotNull]
		public Task CommitChanges()
		{
			if (_changes.IsTrackingChanges)
			{
				var oldChanges = _changes;
				_changes = oldChanges.Next;
				return oldChanges.CommitAll();
			}
			return _StorageSink.CompletedTask;
		}

		public void StartTrackingChanges()
		{
			if (!_changes.IsTrackingChanges)
			{
				_changes = new _UndoWithChangeTracking(_disk);
			}
		}

		[NotNull]
		public _Storage Clone()
		{
			return new _Storage(_allFiles, _disk);
		}

		[NotNull]
		public async Task<FsDirectory> UndoCache([NotNull] FileSystem fileSystem)
		{
			var undoWithChangeTracking = _changes as _UndoWithChangeTracking;
			if (undoWithChangeTracking == null)
				return null;
			return fileSystem.Directory(await undoWithChangeTracking.UndoDataCache);
		}
	}
}
