// SimulatableAPI
// File: DirectoryModifier.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Simulated._Fs
{
	public class DirectoryModifier
	{
		[NotNull] private readonly FileSystem _allFiles;

		public DirectoryModifier([NotNull] FileSystem allFiles)
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
			return (await _allFiles._Disk.FindFiles(path, searchPattern)).Select(p => new FsFile(_allFiles, p));
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
	}
}
