// SimulatableAPI
// File: _IDisk.cs
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
		bool DirExistsNeedsToBeMadeDelayStart([NotNull] FsPath path);

		[NotNull]
		Task<bool> FileExistsNeedsToBeMadeDelayStart([NotNull] FsPath path);

		[NotNull]
		Task<string> TextContentsNeedsToBeMadeDelayStart([NotNull] FsPath path);

		[NotNull]
		IObservable<byte[]> RawContentsNeedsToBeMadeDelayStart([NotNull] FsPath path);

		void CreateDirNeedsToBeMadeDelayStart([NotNull] FsPath path);

		[NotNull]
		Task OverwriteNeedsToBeMadeDelayStart([NotNull] FsPath path, [NotNull] string newContents);

		void OverwriteNeedsToBeMadeDelayStart([NotNull] FsPath path, [NotNull] byte[] newContents);
		void DeleteDirNeedsToBeMadeDelayStart([NotNull] FsPath path);
		void DeleteFileNeedsToBeMadeDelayStart([NotNull] FsPath path);
		void MoveFileNeedsToBeMadeDelayStart([NotNull] FsPath src, [NotNull] FsPath dest);
		void MoveDirNeedsToBeMadeDelayStart([NotNull] FsPath src, [NotNull] FsPath dest);

		[NotNull]
		IEnumerable<FsPath> FindFilesNeedsToBeMadeDelayStart([NotNull] FsPath path, [NotNull] string searchPattern);
	}
}
