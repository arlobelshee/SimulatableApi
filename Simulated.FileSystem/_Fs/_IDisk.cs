﻿// SimulatableAPI
// File: _IDisk.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Simulated._Fs
{
	internal interface _IFsDisk
	{
		Task<bool> DirExists([NotNull] FsPath path);
		Task<bool> FileExists([NotNull] FsPath path);
		Task<string> TextContents([NotNull] FsPath path);
		Task<byte[]> RawContents([NotNull] FsPath path);
		Task CreateDir([NotNull] FsPath path);
		Task Overwrite([NotNull] FsPath path, [NotNull] string newContents);
		Task Overwrite([NotNull] FsPath path, [NotNull] byte[] newContents);
		Task DeleteDir([NotNull] FsPath path);
		Task DeleteFile([NotNull] FsPath path);
		Task MoveFile([NotNull] FsPath src, [NotNull] FsPath dest);
		Task MoveDir(FsPath src, FsPath dest);
		Task<IEnumerable<FsPath>> FindFiles([NotNull] FsPath path, [NotNull] string searchPattern);
	}
}
