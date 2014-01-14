// SimulatableAPI
// File: _IDisk.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Simulated._Fs
{
	internal interface _IFsDisk
	{
		[NotNull]
		Task<bool> DirExists([NotNull] FsPath path);

		[NotNull]
		Task<bool> FileExists([NotNull] FsPath path);

		[NotNull]
		Task<string> TextContents([NotNull] FsPath path);

		[NotNull]
		Task<byte[]> RawContents([NotNull] FsPath path);

		[NotNull]
		Task CreateDir([NotNull] FsPath path);

		[NotNull]
		Task Overwrite([NotNull] FsPath path, [NotNull] string newContents);

		[NotNull]
		Task Overwrite([NotNull] FsPath path, [NotNull] byte[] newContents);

		[NotNull]
		Task DeleteDir([NotNull] FsPath path);

		[NotNull]
		Task DeleteFile([NotNull] FsPath path);

		[NotNull]
		Task MoveFile([NotNull] FsPath src, [NotNull] FsPath dest);

		[NotNull]
		Task MoveDir([NotNull] FsPath src, [NotNull] FsPath dest);

		[NotNull]
		Task<IEnumerable<FsPath>> FindFiles([NotNull] FsPath path, [NotNull] string searchPattern);
	}
}
