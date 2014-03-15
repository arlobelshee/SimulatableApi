﻿// SimulatableAPI
// File: _DiskReal.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Simulated._Fs
{
	internal class _DiskReal : _IFsDisk
	{
		public bool DirExists(FsPath path)
		{
			return Directory.Exists(path._Absolute);
		}

		public bool FileExists(FsPath path)
		{
			return File.Exists(path._Absolute);
		}

		public async Task<string> TextContents(FsPath path)
		{
			_ValidatePathForReadingFile(path);
			using (var contents = File.OpenText(path._Absolute))
			{
				return await contents.ReadToEndAsync();
			}
		}

		public IObservable<byte[]> RawContents(FsPath path)
		{
			const int bufferSize = 1024 * 10;
			return Observable.Create<byte[]>(obs =>
			{
				var isCancelled = false;
				Action execution = async () =>
				{
					try
					{
						_ValidatePathForReadingFile(path);
						using (var contents = File.OpenRead(path._Absolute))
						{
							var buffer = new byte[bufferSize];
							int numRead;
							while (0 != (numRead = await contents.ReadAsync(buffer, 0, buffer.Length)))
							{
								if (isCancelled)
									return;
								if (numRead < buffer.Length)
								{
									var toSend = new byte[numRead];
									Buffer.BlockCopy(buffer, 0, toSend, 0, numRead);
									obs.OnNext(toSend);
								}
								else
								{
									obs.OnNext(buffer);
									buffer = new byte[bufferSize];
								}
							}
						}
					}
					catch (Exception ex)
					{
						obs.OnError(ex);
					}
					finally
					{
						obs.OnCompleted();
					}
				};
				execution();
				return () => isCancelled = true;
			});
		}

		public void CreateDir(FsPath path)
		{
			Directory.CreateDirectory(path._Absolute);
		}

		public void Overwrite(FsPath path, string newContents)
		{
			CreateDir(path.Parent);
			File.WriteAllText(path._Absolute, newContents);
		}

		public void Overwrite(FsPath path, byte[] newContents)
		{
			CreateDir(path.Parent);
			File.WriteAllBytes(path._Absolute, newContents);
		}

		public void DeleteDir(FsPath path)
		{
			if (FileExists(path))
				throw new BadStorageRequest(string.Format(UserMessages.DeleteErrorDeletedFileAsDirectory, path));
			if (DirExists(path))
				Directory.Delete(path._Absolute, true);
		}

		public void DeleteFile(FsPath path)
		{
			if (DirExists(path))
				throw new BadStorageRequest(string.Format(UserMessages.DeleteErrorDeletedDirectoryAsFile, path));
			File.Delete(path._Absolute);
		}

		public void MoveFile(FsPath src, FsPath dest)
		{
			if (DirExists(src))
				throw new BadStorageRequest(string.Format(UserMessages.MoveErrorMovedDirectoryAsFile, src._Absolute, dest._Absolute));
			if (!FileExists(src))
				throw new BadStorageRequest(string.Format(UserMessages.MoveErrorMissingSource, src._Absolute, dest._Absolute));
			if (FileExists(dest) || DirExists(dest))
				throw new BadStorageRequest(string.Format(UserMessages.MoveErrorDestinationBlocked, src._Absolute, dest._Absolute));
			CreateDir(dest.Parent);
			File.Move(src._Absolute, dest._Absolute);
		}

		public void MoveDir(FsPath src, FsPath dest)
		{
			if (FileExists(src))
				throw new BadStorageRequest(string.Format(UserMessages.MoveErrorMovedFileAsDirectory, src._Absolute));
			if (!DirExists(src))
				throw new BadStorageRequest(string.Format(UserMessages.MoveErrorMissingSource, src._Absolute));
			if (FileExists(dest) || DirExists(dest))
				throw new BadStorageRequest(string.Format(UserMessages.MoveErrorDestinationBlocked, src._Absolute, dest._Absolute));
			Directory.Move(src._Absolute, dest._Absolute);
		}

		public IEnumerable<FsPath> FindFiles(FsPath path, string searchPattern)
		{
			if (!DirExists(path))
				return Enumerable.Empty<FsPath>();
			return Directory.EnumerateFiles(path._Absolute, searchPattern)
				.Select(p => path/Path.GetFileName(p));
		}

		private void _ValidatePathForReadingFile([NotNull] FsPath path)
		{
			if (DirExists(path))
				throw new BadStorageRequest(string.Format(UserMessages.ReadErrorPathIsDirectory, path));
			if (!FileExists(path))
				throw new BadStorageRequest(string.Format(UserMessages.ReadErrorFileNotFound, path));
		}
	}
}
