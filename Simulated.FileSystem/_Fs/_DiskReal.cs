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
	internal class _DiskReal : _StorageSink
	{
		public override Task<bool> DirExists(FsPath path)
		{
			return Directory.Exists(path.Absolute)
				.AsImmediateTask();
		}

		public override Task<bool> FileExists(FsPath path)
		{
			return File.Exists(path.Absolute)
				.AsImmediateTask();
		}

		public override Task<string> TextContents(FsPath path)
		{
			return Task.Run(() => File.ReadAllText(path.Absolute));
		}

		public override Task<byte[]> RawContents(FsPath path)
		{
			return Task.Run(() => File.ReadAllBytes(path.Absolute));
		}

		public override Task CreateDir(FsPath path)
		{
			return Task.Run(() => Directory.CreateDirectory(path.Absolute));
		}

		public override async Task Overwrite(FsPath path, string newContents)
		{
			using (var writer = File.CreateText(path.Absolute))
			{
				await writer.WriteAsync(newContents);
				await writer.FlushAsync();
			}
		}

		public override async Task Overwrite(FsPath path, byte[] newContents)
		{
			using (var writer = File.OpenWrite(path.Absolute))
			{
				await writer.WriteAsync(newContents, 0, newContents.Length);
				await writer.FlushAsync();
			}
		}

		public override Task DeleteDir(FsPath path)
		{
			return Task.Run(() => Directory.Delete(path.Absolute, true));
		}

		public override Task DeleteFile(FsPath path)
		{
			return Task.Run(() => File.Delete(path.Absolute));
		}

		public override Task MoveFile(FsPath src, FsPath dest)
		{
			return Task.Run(() => File.Move(src.Absolute, dest.Absolute));
		}

		public override Task MoveDir(FsPath src, FsPath dest)
		{
			return Task.Run(() => Directory.Move(src.Absolute, dest.Absolute));
		}

		public override async Task<IEnumerable<FsPath>> FindFiles(FsPath path, string searchPattern)
		{
			if (!await DirExists(path))
				return Enumerable.Empty<FsPath>();
			return await Task.Run(()=>Directory.EnumerateFiles(path.Absolute, searchPattern)
				.Select(p => new FsPath(p)));
		}
	}
}
