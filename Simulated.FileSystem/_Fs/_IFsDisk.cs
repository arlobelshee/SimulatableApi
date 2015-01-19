// SimulatableAPI
// File: _IFsDisk.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Simulated._Fs
{
	/// <summary>
	///    Disks expose low-level operations. Each method takes no action when called. Instead, it returns a task which may be
	///    started at any time the caller desires.
	/// </summary>
	internal interface _IFsDisk
	{
		bool DirExists([NotNull] FsPath path);

		[NotNull]
		Task<bool> FileExists([NotNull] FsPath path);

		[NotNull]
		Task<string> TextContents([NotNull] FsPath path);

		[NotNull]
		IObservable<byte[]> RawContents([NotNull] FsPath path);

		[NotNull]
		Task CreateDir([NotNull] FsPath path);

		[NotNull]
		Task DeleteDir([NotNull] FsPath path);

		[NotNull]
		Task Overwrite([NotNull] FsPath path, [NotNull] string newContents);

		[NotNull]
		Task Overwrite([NotNull] FsPath path, [NotNull] byte[] newContents);

		void DeleteFile([NotNull] FsPath path);
		void MoveFile([NotNull] FsPath src, [NotNull] FsPath dest);
		void MoveDir([NotNull] FsPath src, [NotNull] FsPath dest);

		[NotNull]
		IEnumerable<FsPath> FindFiles([NotNull] FsPath path, [NotNull] string searchPattern);
	}
}
