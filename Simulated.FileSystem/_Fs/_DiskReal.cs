// SimulatableAPI
// File: _DiskReal.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;

namespace Simulated._Fs
{
	internal class _DiskReal : _IFsDisk
	{
		public bool DirExists(FsPath path)
		{
			return Directory.Exists(path.Absolute);
		}

		public bool FileExists(FsPath path)
		{
			return File.Exists(path.Absolute);
		}

		public string TextContents(FsPath path)
		{
			_ValidatePathForReadingFile(path);
			return File.ReadAllText(path.Absolute);
		}

		public byte[] RawContents(FsPath path)
		{
			_ValidatePathForReadingFile(path);
			return File.ReadAllBytes(path.Absolute);
		}

		public void CreateDir(FsPath path)
		{
			Directory.CreateDirectory(path.Absolute);
		}

		public void Overwrite(FsPath path, string newContents)
		{
			CreateDir(path.Parent);
			File.WriteAllText(path.Absolute, newContents);
		}

		public void Overwrite(FsPath path, byte[] newContents)
		{
			CreateDir(path.Parent);
			File.WriteAllBytes(path.Absolute, newContents);
		}

		public void DeleteDir(FsPath path)
		{
			if (DirExists(path))
				Directory.Delete(path.Absolute, true);
		}

		public void DeleteFile(FsPath path)
		{
			File.Delete(path.Absolute);
		}

		public void MoveFile(FsPath src, FsPath dest)
		{
			if (DirExists(src))
				throw new BadStorageRequest(string.Format(UserMessages.MoveErrorMovedDirectoryAsFile, src.Absolute, dest.Absolute));
			if (!FileExists(src))
				throw new BadStorageRequest(string.Format(UserMessages.MoveErrorMissingSource, src.Absolute, dest.Absolute));
			if (FileExists(dest) || DirExists(dest))
				throw new BadStorageRequest(string.Format(UserMessages.MoveErrorDestinationBlocked, src.Absolute, dest.Absolute));
			CreateDir(dest.Parent);
			File.Move(src.Absolute, dest.Absolute);
		}

		public void MoveDir(FsPath src, FsPath dest)
		{
			if (FileExists(src))
				throw new BadStorageRequest(string.Format(UserMessages.MoveErrorMovedFileAsDirectory, src.Absolute));
			if (!DirExists(src))
				throw new BadStorageRequest(string.Format(UserMessages.MoveErrorMissingSource, src.Absolute));
			if (FileExists(dest) || DirExists(dest))
				throw new BadStorageRequest(string.Format(UserMessages.MoveErrorDestinationBlocked, src.Absolute, dest.Absolute));
			Directory.Move(src.Absolute, dest.Absolute);
		}

		public IEnumerable<FsPath> FindFiles(FsPath path, string searchPattern)
		{
			if (!DirExists(path))
				return Enumerable.Empty<FsPath>();
			return Directory.EnumerateFiles(path.Absolute, searchPattern)
				.Select(p => new FsPath(p));
		}

		private void _ValidatePathForReadingFile([NotNull] FsPath path)
		{
			if (!FileExists(path) && !DirExists(path))
				throw new BadStorageRequest(string.Format(UserMessages.ReadErrorFileNotFound, path));
		}
	}
}
