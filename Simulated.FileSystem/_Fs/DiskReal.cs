// SimulatableAPI
// File: DiskReal.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System.Collections.Generic;
using System.IO;
using System.Linq;

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
			return File.ReadAllText(path.Absolute);
		}

		public byte[] RawContents(FsPath path)
		{
			return File.ReadAllBytes(path.Absolute);
		}

		public void CreateDir(FsPath path)
		{
			Directory.CreateDirectory(path.Absolute);
		}

		public void Overwrite(FsPath path, string newContents)
		{
			File.WriteAllText(path.Absolute, newContents);
		}

		public void Overwrite(FsPath path, byte[] newContents)
		{
			File.WriteAllBytes(path.Absolute, newContents);
		}

		public void DeleteDir(FsPath path)
		{
			Directory.Delete(path.Absolute, true);
		}

		public void DeleteFile(FsPath path)
		{
			File.Delete(path.Absolute);
		}

		public void MoveFile(FsPath src, FsPath dest)
		{
			File.Move(src.Absolute, dest.Absolute);
		}

		public void MoveDir(FsPath src, FsPath dest)
		{
			Directory.Move(src.Absolute, dest.Absolute);
		}

		public IEnumerable<FsPath> FindFiles(FsPath path, string searchPattern)
		{
			if (!DirExists(path))
				return Enumerable.Empty<FsPath>();
			return Directory.EnumerateFiles(path.Absolute, searchPattern)
				.Select(p => new FsPath(p));
		}
	}
}
