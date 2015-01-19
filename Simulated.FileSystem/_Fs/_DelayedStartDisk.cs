// SimulatableAPI
// File: _DelayedStartDisk.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Simulated._Fs
{
	internal class _DelayedStartDisk
	{
		[NotNull] private readonly _IFsDisk _storage;

		public _DelayedStartDisk([NotNull] _IFsDisk storage)
		{
			_storage = storage;
		}

		[NotNull]
		public Task<bool> FileExistsNeedsToBeMadeDelayStart([NotNull] FsPath location)
		{
			return new Task<bool>(()=>_storage.FileExists(location).Result);
		}

		[NotNull]
		public Task Overwrite([NotNull] FsPath location, [NotNull] string contents)
		{
			return new Task(() => _storage.Overwrite(location,contents).Wait());
		}
	}
}
