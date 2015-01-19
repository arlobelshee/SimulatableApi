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
		public bool DirExists(FsPath path)
		{
			return Directory.Exists(path._Absolute);
		}

		public Task<bool> FileExists(FsPath path)
		{
			return File.Exists(path._Absolute)
				.AsTask();
		}

		public async Task<string> TextContents(FsPath path)
		{
			_ValidatePathForReadingFile(path);
			using (var contents = File.OpenText(path._Absolute))
			{
				return await contents.ReadToEndAsync()
					.ConfigureAwait(false);
			}
		}

		public IObservable<byte[]> RawContents(FsPath path)
		{
			const int bufferSize = 1024*10;
			return _Make.Observable<byte[]>(async ctx =>
			{
				_ValidatePathForReadingFile(path);
				using (var contents = File.OpenRead(path._Absolute))
				{
					var buffer = new byte[bufferSize];
					int bytesRead;
					while (0 != (bytesRead = await contents.ReadAsync(buffer, 0, buffer.Length, ctx.CancelToken)
						.ConfigureAwait(false)))
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

#pragma warning disable 1998
		public async Task CreateDir(FsPath path)
#pragma warning restore 1998
		{
			if (File.Exists(path._Absolute))
				throw new BadStorageRequest(string.Format(UserMessages.CreateErrorCreatedDirectoryOnTopOfFile, path));
			Directory.CreateDirectory(path._Absolute);
		}

#pragma warning disable 1998
		public async Task DeleteDir(FsPath path)
#pragma warning restore 1998
		{
			if (FileExists(path)
				.TemporaryUnwrapWhileIRefactorIncrementally())
				throw new BadStorageRequest(string.Format(UserMessages.DeleteErrorDeletedFileAsDirectory, path));
			if (DirExists(path))
				Directory.Delete(path._Absolute, true);
		}

		public async Task Overwrite(FsPath path, string newContents)
		{
			if (Directory.Exists(path._Absolute))
				throw new BadStorageRequest(string.Format(UserMessages.WriteErrorPathIsDirectory, path._Absolute));
			await CreateDir(path.Parent);
			using (var contents = File.CreateText(path._Absolute))
			{
				await contents.WriteAsync(newContents)
					.ConfigureAwait(false);
			}
		}

		public async Task Overwrite(FsPath path, byte[] newContents)
		{
			if (Directory.Exists(path._Absolute))
				throw new BadStorageRequest(string.Format(UserMessages.WriteErrorPathIsDirectory, path._Absolute));
			await CreateDir(path.Parent);
			File.WriteAllBytes(path._Absolute, newContents);
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
			if (!FileExists(src)
				.TemporaryUnwrapWhileIRefactorIncrementally())
				throw new BadStorageRequest(string.Format(UserMessages.MoveErrorMissingSource, src._Absolute, dest._Absolute));
			if (FileExists(dest)
				.TemporaryUnwrapWhileIRefactorIncrementally() || DirExists(dest))
				throw new BadStorageRequest(string.Format(UserMessages.MoveErrorDestinationBlocked, src._Absolute, dest._Absolute));
			CreateDir(dest.Parent)
				.RunSynchronouslyAsCheapHackUntilIFixScheduling();
			File.Move(src._Absolute, dest._Absolute);
		}

		public void MoveDir(FsPath src, FsPath dest)
		{
			if (FileExists(src)
				.TemporaryUnwrapWhileIRefactorIncrementally())
				throw new BadStorageRequest(string.Format(UserMessages.MoveErrorMovedFileAsDirectory, src._Absolute));
			if (!DirExists(src))
				throw new BadStorageRequest(string.Format(UserMessages.MoveErrorMissingSource, src._Absolute));
			if (FileExists(dest)
				.TemporaryUnwrapWhileIRefactorIncrementally() || DirExists(dest))
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
			if (!FileExists(path)
				.TemporaryUnwrapWhileIRefactorIncrementally())
				throw new BadStorageRequest(string.Format(UserMessages.ReadErrorFileNotFound, path));
		}
	}
}
