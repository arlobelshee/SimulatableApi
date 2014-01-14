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
	public class Storage
	{
		[NotNull] private readonly FileSystem _allFiles;

		public Storage([NotNull] FileSystem allFiles)
		{
			_allFiles = allFiles;
		}

		[NotNull]
		public Task<bool> IsDirectory([NotNull] FsPath path)
		{
			return _allFiles._Disk.DirExists(path);
		}

		[NotNull]
		public async Task EnsureDirectoryExists([NotNull] FsPath path)
		{
			if (await IsDirectory(path))
				return;
			_AllMissingDirectoriesInPathFromBottomUp(path)
				.Reverse()
				.Each(dir => _allFiles._Changes.CreatedDirectory(new FsPath(dir)));
			await _allFiles._Disk.CreateDir(path);
		}

		[NotNull]
		public async Task EnsureDirectoryDoesNotExist([NotNull] FsPath path)
		{
			if (!await IsDirectory(path))
				return;
			await _allFiles._Changes.DeletedDirectory(path);
			if (await IsDirectory(path))
				await _allFiles._Disk.DeleteDir(path);
		}

		[NotNull]
		public async Task<IEnumerable<FsFile>> KnownFilesIn([NotNull] string searchPattern, [NotNull] FsPath path)
		{
			return (await _allFiles._Disk.FindFiles(path, searchPattern)).Select(p => new FsFile(_allFiles, p, this));
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

		public Task<bool> DoFileExists(FsPath path)
		{
			return _allFiles._Disk.FileExists(path);
		}

		[NotNull]
		public async Task DoOverwriteFileContents([NotNull] FsPath path, [NotNull] string newContents, [NotNull] FsDirectory parent)
		{
			await parent.EnsureExists();
			await _allFiles._Changes.Overwrote(path);
			await _allFiles._Disk.Overwrite(path, newContents);
		}

		[NotNull]
		public async Task DoOverwriteFileContentsBinary([NotNull] FsPath path, [NotNull] byte[] newContents, [NotNull] FsDirectory parent)
		{
			await parent.EnsureExists();
			await _allFiles._Changes.Overwrote(path);
			await _allFiles._Disk.Overwrite(path, newContents);
		}

		[NotNull]
		public Task<string> TextContents([NotNull] FsPath path)
		{
			return _allFiles._Disk.TextContents(path);
		}

		[NotNull]
		public Task<byte[]> RawContents([NotNull] FsPath path)
		{
			return _allFiles._Disk.RawContents(path);
		}
	}
}
