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
			return File.Exists(path.Absolute)
				.AsImmediateTask();
		}

		public Task<string> TextContents(FsPath path)
		{
			return Task.Run(() => File.ReadAllText(path.Absolute));
		}

		public Task<byte[]> RawContents(FsPath path)
		{
			return Task.Run(() => File.ReadAllBytes(path.Absolute));
		}

		public Task CreateDir(FsPath path)
		{
			return Task.Run(()=>Directory.CreateDirectory(path.Absolute));
		}

		public async Task Overwrite(FsPath path, string newContents)
		{
			using (var writer = File.CreateText(path.Absolute))
			{
				await writer.WriteAsync(newContents);
				await writer.FlushAsync();
			}
		}

		public async Task Overwrite(FsPath path, byte[] newContents)
		{
			using (var writer = File.OpenWrite(path.Absolute))
			{
				await writer.WriteAsync(newContents, 0, newContents.Length);
				await writer.FlushAsync();
			}
		}

		public Task DeleteDir(FsPath path)
		{
			return Task.Run(()=>Directory.Delete(path.Absolute, true));
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

		public async Task<IEnumerable<FsPath>> FindFiles(FsPath path, string searchPattern)
		{
			if (!await DirExists(path))
				return Enumerable.Empty<FsPath>();
			return Directory.EnumerateFiles(path.Absolute, searchPattern)
				.Select(p => new FsPath(p));
		}
	}
}
