// SimulatableAPI
// File: _DiskReal.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Simulated._Fs
{
	internal class _DiskReal : _IFsDisk
	{
		public bool DirExistsNeedsToBeMadeDelayStart(FsPath path)
		{
			return Directory.Exists(path._Absolute);
		}

		public Task<bool> FileExistsNeedsToBeMadeDelayStart(FsPath path)
		{
			return File.Exists(path._Absolute).AsTask();
		}

		public async Task<string> TextContentsNeedsToBeMadeDelayStart(FsPath path)
		{
			_ValidatePathForReadingFile(path);
			using (var contents = File.OpenText(path._Absolute))
			{
				return await contents.ReadToEndAsync().ConfigureAwait(false);
			}
		}

		public IObservable<byte[]> RawContentsNeedsToBeMadeDelayStart(FsPath path)
		{
			const int bufferSize = 1024*10;
			return _Make.Observable<byte[]>(async ctx =>
			{
				_ValidatePathForReadingFile(path);
				using (var contents = File.OpenRead(path._Absolute))
				{
					var buffer = new byte[bufferSize];
					int bytesRead;
					while (0 != (bytesRead = await contents.ReadAsync(buffer, 0, buffer.Length, ctx.CancelToken).ConfigureAwait(false)))
					{
						if (ctx.IsCancelled)
							return;
						if (bytesRead < buffer.Length)
						{
							var toSend = new byte[bytesRead];
							Buffer.BlockCopy(buffer, 0, toSend, 0, bytesRead);
							ctx.OnNext(toSend);
						}
						else
						{
							ctx.OnNext(buffer);
							buffer = new byte[bufferSize];
						}
					}
				}
			});
		}

		public void CreateDirNeedsToBeMadeDelayStart(FsPath path)
		{
			Directory.CreateDirectory(path._Absolute);
		}

		public async Task OverwriteNeedsToBeMadeDelayStart(FsPath path, string newContents)
		{
			CreateDirNeedsToBeMadeDelayStart(path.Parent);
			using (var contents = File.CreateText(path._Absolute))
			{
				await contents.WriteAsync(newContents).ConfigureAwait(false);
			}
		}

		public void OverwriteNeedsToBeMadeDelayStart(FsPath path, byte[] newContents)
		{
			CreateDirNeedsToBeMadeDelayStart(path.Parent);
			File.WriteAllBytes(path._Absolute, newContents);
		}

		public void DeleteDirNeedsToBeMadeDelayStart(FsPath path)
		{
			if (_TemporaryUnwrapWhileIRefactorIncrementally(FileExistsNeedsToBeMadeDelayStart(path)))
				throw new BadStorageRequest(string.Format(UserMessages.DeleteErrorDeletedFileAsDirectory, path));
			if (DirExistsNeedsToBeMadeDelayStart(path))
				Directory.Delete(path._Absolute, true);
		}

		private bool _TemporaryUnwrapWhileIRefactorIncrementally(Task<bool> fileExists)
		{
			return fileExists.Result;
		}

		public void DeleteFileNeedsToBeMadeDelayStart(FsPath path)
		{
			if (DirExistsNeedsToBeMadeDelayStart(path))
				throw new BadStorageRequest(string.Format(UserMessages.DeleteErrorDeletedDirectoryAsFile, path));
			File.Delete(path._Absolute);
		}

		public void MoveFileNeedsToBeMadeDelayStart(FsPath src, FsPath dest)
		{
			if (DirExistsNeedsToBeMadeDelayStart(src))
				throw new BadStorageRequest(string.Format(UserMessages.MoveErrorMovedDirectoryAsFile, src._Absolute, dest._Absolute));
			if (!_TemporaryUnwrapWhileIRefactorIncrementally(FileExistsNeedsToBeMadeDelayStart(src)))
				throw new BadStorageRequest(string.Format(UserMessages.MoveErrorMissingSource, src._Absolute, dest._Absolute));
			if (_TemporaryUnwrapWhileIRefactorIncrementally(FileExistsNeedsToBeMadeDelayStart(dest)) || DirExistsNeedsToBeMadeDelayStart(dest))
				throw new BadStorageRequest(string.Format(UserMessages.MoveErrorDestinationBlocked, src._Absolute, dest._Absolute));
			CreateDirNeedsToBeMadeDelayStart(dest.Parent);
			File.Move(src._Absolute, dest._Absolute);
		}

		public void MoveDirNeedsToBeMadeDelayStart(FsPath src, FsPath dest)
		{
			if (_TemporaryUnwrapWhileIRefactorIncrementally(FileExistsNeedsToBeMadeDelayStart(src)))
				throw new BadStorageRequest(string.Format(UserMessages.MoveErrorMovedFileAsDirectory, src._Absolute));
			if (!DirExistsNeedsToBeMadeDelayStart(src))
				throw new BadStorageRequest(string.Format(UserMessages.MoveErrorMissingSource, src._Absolute));
			if (_TemporaryUnwrapWhileIRefactorIncrementally(FileExistsNeedsToBeMadeDelayStart(dest)) || DirExistsNeedsToBeMadeDelayStart(dest))
				throw new BadStorageRequest(string.Format(UserMessages.MoveErrorDestinationBlocked, src._Absolute, dest._Absolute));
			Directory.Move(src._Absolute, dest._Absolute);
		}

		public IEnumerable<FsPath> FindFilesNeedsToBeMadeDelayStart(FsPath path, string searchPattern)
		{
			if (!DirExistsNeedsToBeMadeDelayStart(path))
				return Enumerable.Empty<FsPath>();
			return Directory.EnumerateFiles(path._Absolute, searchPattern)
				.Select(p => path/Path.GetFileName(p));
		}

		private void _ValidatePathForReadingFile([NotNull] FsPath path)
		{
			if (DirExistsNeedsToBeMadeDelayStart(path))
				throw new BadStorageRequest(string.Format(UserMessages.ReadErrorPathIsDirectory, path));
			if (!_TemporaryUnwrapWhileIRefactorIncrementally(FileExistsNeedsToBeMadeDelayStart(path)))
				throw new BadStorageRequest(string.Format(UserMessages.ReadErrorFileNotFound, path));
		}
	}
}
