// SimulatableAPI
// File: DiskReal.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Simulated._Fs
{
	internal class _DiskReal : _IFsDisk
	{
		public Task<bool> DirExists(FsPath path)
		{
			return Directory.Exists(path.Absolute)
				.AsImmediateTask();
		}

		public Task<bool> FileExists(FsPath path)
		{
			return File.Exists(path.Absolute).AsImmediateTask();
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

		public Task<IEnumerable<FsPath>> FindFiles(FsPath path, string searchPattern)
		{
			return DirExists(path)
				.ContinueWith(r => _FindFilesImpl(path, searchPattern, r.Result));
		}

		private IEnumerable<FsPath> _FindFilesImpl(FsPath path, string searchPattern, bool dirExists)
		{
			if (!dirExists)
				return Enumerable.Empty<FsPath>();
			return Directory.EnumerateFiles(path.Absolute, searchPattern)
				.Select(p => new FsPath(p));
		}
	}
}
