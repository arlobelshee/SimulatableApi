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
		public Task<bool> DirExists(FsPath path)
		{
			return Directory.Exists(path._Absolute)
				.AsTask();
		}

		public Task<bool> FileExists(FsPath path)
		{
			return File.Exists(path._Absolute)
				.AsTask();
		}

		public async Task<string> TextContents(FsPath path)
		{
			await _ValidatePathForReadingFile(path)
				.ConfigureAwait(false);
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
				await _ValidatePathForReadingFile(path)
					.ConfigureAwait(false);
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
			if (await FileExists(path)
				.ConfigureAwait(false))
				throw new BadStorageRequest(string.Format(UserMessages.CreateErrorCreatedDirectoryOnTopOfFile, path));
			Directory.CreateDirectory(path._Absolute);
		}

#pragma warning disable 1998
		public async Task DeleteDir(FsPath path)
#pragma warning restore 1998
		{
			if (await FileExists(path)
				.ConfigureAwait(false))
				throw new BadStorageRequest(string.Format(UserMessages.DeleteErrorDeletedFileAsDirectory, path));
			if (await DirExists(path)
				.ConfigureAwait(false))
				Directory.Delete(path._Absolute, true);
		}

		public async Task Overwrite(FsPath path, string newContents)
		{
			if (await DirExists(path)
				.ConfigureAwait(false))
				throw new BadStorageRequest(string.Format(UserMessages.WriteErrorPathIsDirectory, path._Absolute));
			await CreateDir(path.Parent)
				.ConfigureAwait(false);
			using (var contents = File.CreateText(path._Absolute))
			{
				await contents.WriteAsync(newContents)
					.ConfigureAwait(false);
			}
		}

		public async Task Overwrite(FsPath path, byte[] newContents)
		{
			if (await DirExists(path)
				.ConfigureAwait(false))
				throw new BadStorageRequest(string.Format(UserMessages.WriteErrorPathIsDirectory, path._Absolute));
			await CreateDir(path.Parent)
				.ConfigureAwait(false);
			File.WriteAllBytes(path._Absolute, newContents);
		}

		public async Task DeleteFile(FsPath path)
		{
			if (await DirExists(path)
				.ConfigureAwait(false))
				throw new BadStorageRequest(string.Format(UserMessages.DeleteErrorDeletedDirectoryAsFile, path));
			File.Delete(path._Absolute);
		}

		public async Task MoveFile(FsPath src, FsPath dest)
		{
			if (await DirExists(src)
				.ConfigureAwait(false))
				throw new BadStorageRequest(string.Format(UserMessages.MoveErrorMovedDirectoryAsFile, src._Absolute, dest._Absolute));
			if (!await FileExists(src)
				.ConfigureAwait(false))
				throw new BadStorageRequest(string.Format(UserMessages.MoveErrorMissingSource, src._Absolute, dest._Absolute));
			if (await FileExists(dest)
				.ConfigureAwait(false) || await DirExists(dest)
					.ConfigureAwait(false))
				throw new BadStorageRequest(string.Format(UserMessages.MoveErrorDestinationBlocked, src._Absolute, dest._Absolute));
			await CreateDir(dest.Parent);
			File.Move(src._Absolute, dest._Absolute);
		}

		public async Task MoveDir(FsPath src, FsPath dest)
		{
			if (await FileExists(src)
				.ConfigureAwait(false))
				throw new BadStorageRequest(string.Format(UserMessages.MoveErrorMovedFileAsDirectory, src._Absolute));
			if (!await DirExists(src)
				.ConfigureAwait(false))
				throw new BadStorageRequest(string.Format(UserMessages.MoveErrorMissingSource, src._Absolute));
			if (await FileExists(dest)
				.ConfigureAwait(false) || await DirExists(dest)
					.ConfigureAwait(false))
				throw new BadStorageRequest(string.Format(UserMessages.MoveErrorDestinationBlocked, src._Absolute, dest._Absolute));
			Directory.Move(src._Absolute, dest._Absolute);
		}

		public IObservable<FsPath> FindFiles(FsPath path, string searchPattern)
		{
			return _Make.Observable<FsPath>(async ctx =>
			{
				if (!await DirExists(path).ConfigureAwait(false))
					return;
				foreach (var file in Directory.EnumerateFiles(path._Absolute, searchPattern))
				{
					if (ctx.IsCancelled)
						return;
					ctx.OnNext(path / Path.GetFileName(file));
				}
			});
		}

		[NotNull]
		private async Task _ValidatePathForReadingFile([NotNull] FsPath path)
		{
			if (await DirExists(path)
				.ConfigureAwait(false))
				throw new BadStorageRequest(string.Format(UserMessages.ReadErrorPathIsDirectory, path));
			if (!await FileExists(path)
				.ConfigureAwait(false))
				throw new BadStorageRequest(string.Format(UserMessages.ReadErrorFileNotFound, path));
		}
	}
}
