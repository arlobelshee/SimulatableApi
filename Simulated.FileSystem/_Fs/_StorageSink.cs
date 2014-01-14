// SimulatableAPI
// File: _StorageSink.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Simulated._Fs
{
	internal abstract class _StorageSink
	{
		public virtual bool IsTrackingChanges
		{
			get { return false; }
		}

		[NotNull]
		public virtual _StorageSink Next
		{
			get { throw new NotImplementedException(); }
		}

		[NotNull]
		public virtual Task CommitAll()
		{
			throw new NotImplementedException();
		}

		[NotNull]
		public virtual Task RevertAll()
		{
			throw new NotImplementedException();
		}

		public virtual void CreatedDirectory([NotNull] FsPath path)
		{
			throw new NotImplementedException();
		}

		[NotNull]
		public virtual Task<bool> DirExists([NotNull] FsPath path)
		{
			throw new NotImplementedException();
		}

		[NotNull]
		public virtual Task<bool> FileExists([NotNull] FsPath path)
		{
			throw new NotImplementedException();
		}

		[NotNull]
		public virtual Task<string> TextContents([NotNull] FsPath path)
		{
			throw new NotImplementedException();
		}

		[NotNull]
		public virtual Task<byte[]> RawContents([NotNull] FsPath path)
		{
			throw new NotImplementedException();
		}

		[NotNull]
		public virtual Task CreateDir([NotNull] FsPath path)
		{
			throw new NotImplementedException();
		}

		[NotNull]
		public virtual Task Overwrite([NotNull] FsPath path, [NotNull] string newContents)
		{
			throw new NotImplementedException();
		}

		[NotNull]
		public virtual Task Overwrite([NotNull] FsPath path, [NotNull] byte[] newContents)
		{
			throw new NotImplementedException();
		}

		[NotNull]
		public virtual Task DeleteDir([NotNull] FsPath path)
		{
			throw new NotImplementedException();
		}

		[NotNull]
		public virtual Task DeleteFile([NotNull] FsPath path)
		{
			throw new NotImplementedException();
		}

		[NotNull]
		public virtual Task MoveFile([NotNull] FsPath src, [NotNull] FsPath dest)
		{
			throw new NotImplementedException();
		}

		[NotNull]
		public virtual Task MoveDir([NotNull] FsPath src, [NotNull] FsPath dest)
		{
			throw new NotImplementedException();
		}

		[NotNull]
		public virtual Task<IEnumerable<FsPath>> FindFiles([NotNull] FsPath path, [NotNull] string searchPattern)
		{
			throw new NotImplementedException();
		}

		[NotNull] internal static readonly Task CompletedTask = true.AsImmediateTask();
	}
}
