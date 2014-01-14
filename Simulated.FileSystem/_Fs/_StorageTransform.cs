// SimulatableAPI
// File: _StorageTransform.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Simulated._Fs
{
	internal class _StorageTransform : _StorageSink
	{
		[CanBeNull] private readonly _StorageSink _next;

		protected _StorageTransform([NotNull] _StorageSink next)
		{
			_next = next;
		}

		public override bool IsTrackingChanges
		{
			get { return _next != null && _next.IsTrackingChanges; }
		}

		public override _StorageSink Next
		{
			get { return _next; }
		}

		public override Task CommitAll()
		{
			return _next != null ? _next.CommitAll() : CompletedTask;
		}

		public override Task RevertAll()
		{
			return _next != null ? _next.RevertAll() : CompletedTask;
		}

		public override Task<bool> DirExists(FsPath path)
		{
			return _next != null ? _next.DirExists(path) : false.AsImmediateTask();
		}

		public override Task<bool> FileExists(FsPath path)
		{
			return _next != null ? _next.FileExists(path) : false.AsImmediateTask();
		}

		public override Task<string> TextContents(FsPath path)
		{
			return _next != null ? _next.TextContents(path) : string.Empty.AsImmediateTask();
		}

		public override Task<byte[]> RawContents(FsPath path)
		{
			return _next != null ? _next.RawContents(path) : (new byte[] {}).AsImmediateTask();
		}

		public override Task CreateDir(FsPath path)
		{
			return _next != null ? _next.CreateDir(path) : CompletedTask;
		}

		public override Task Overwrite(FsPath path, string newContents)
		{
			return _next != null ? _next.Overwrite(path, newContents) : CompletedTask;
		}

		public override Task Overwrite(FsPath path, byte[] newContents)
		{
			return _next != null ? _next.Overwrite(path, newContents) : CompletedTask;
		}

		public override Task DeleteDir(FsPath path)
		{
			return _next != null ? _next.DeleteDir(path) : CompletedTask;
		}

		public override Task DeleteFile(FsPath path)
		{
			return _next != null ? _next.DeleteFile(path) : CompletedTask;
		}

		public override Task MoveFile(FsPath src, FsPath dest)
		{
			return _next != null ? _next.MoveFile(src, dest) : CompletedTask;
		}

		public override Task MoveDir(FsPath src, FsPath dest)
		{
			return _next != null ? _next.MoveDir(src, dest) : CompletedTask;
		}

		public override Task<IEnumerable<FsPath>> FindFiles(FsPath path, string searchPattern)
		{
			if (_next != null)
				return _next.FindFiles(path, searchPattern);
			return Enumerable.Empty<FsPath>()
				.AsImmediateTask();
		}
	}
}
