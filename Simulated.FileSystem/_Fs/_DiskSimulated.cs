// SimulatableAPI
// File: _DiskSimulated.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Simulated._Fs
{
	internal class _DiskSimulated : _IFsDisk
	{
		[NotNull] private readonly Dictionary<FsPath, _Node> _data = new Dictionary<FsPath, _Node>();
		[NotNull] private static readonly Encoding DefaultEncoding = Encoding.UTF8;

		public bool DirExistsNeedsToBeMadeDelayStart(FsPath path)
		{
			return _GetStorage(path)
				.Kind == _StorageKind.Directory;
		}

		public Task<bool> FileExistsNeedsToBeMadeDelayStart(FsPath path)
		{
			return (_GetStorage(path)
				.Kind == _StorageKind.File).AsTask();
		}

		public Task<string> TextContentsNeedsToBeMadeDelayStart(FsPath path)
		{
			return Task.Run(() =>
			{
				var storage = _GetStorage(path);
				_ValidateStorage(path, storage);
				return DefaultEncoding.GetString(storage.RawContents);
			});
		}

		public IObservable<byte[]> RawContentsNeedsToBeMadeDelayStart(FsPath path)
		{
			return _Make.Observable<byte[]>(ctx =>
			{
				var storage = _GetStorage(path);
				_ValidateStorage(path, storage);
				ctx.OnNext(storage.RawContents);
			});
		}

		public Task CreateDir(FsPath path)
		{
			return new Task(() =>
			{
				var storage = _GetStorage(path);
				while (storage.Kind != _StorageKind.Directory)
				{
					if (storage.Kind == _StorageKind.File)
						throw new BadStorageRequest(string.Format(UserMessages.CreateErrorCreatedDirectoryOnTopOfFile, path));
					_data[path] = new _Node(_StorageKind.Directory);
					path = path.Parent;
					storage = _GetStorage(path);
				}
			});
		}

		public Task OverwriteNeedsToBeMadeDelayStart(FsPath path, string newContents)
		{
			return Task.Run(() =>
			{
				CreateDir(path.Parent)
					.RunSynchronouslyAsCheapHackUntilIFixScheduling();
				_data[path] = new _Node(_StorageKind.File)
				{
					RawContents = DefaultEncoding.GetBytes(newContents)
				};
			});
		}

		public void OverwriteNeedsToBeMadeDelayStart(FsPath path, byte[] newContents)
		{
			CreateDir(path.Parent)
				.RunSynchronouslyAsCheapHackUntilIFixScheduling();
			_data[path] = new _Node(_StorageKind.File)
			{
				RawContents = newContents
			};
		}

		public Task DeleteDir(FsPath path)
		{
			return new Task(() =>
			{
				var storageKind = _GetStorage(path)
					.Kind;
				if (storageKind == _StorageKind.Missing)
					return;
				if (storageKind == _StorageKind.File)
					throw new BadStorageRequest(string.Format(UserMessages.DeleteErrorDeletedFileAsDirectory, path));
				var toDelete = _ItemsInScopeOfDirectory(path);
				toDelete.Each(p => _data.Remove(p.Key));
			});
		}

		public void DeleteFileNeedsToBeMadeDelayStart(FsPath path)
		{
			var storageKind = _GetStorage(path)
				.Kind;
			if (storageKind == _StorageKind.Missing)
				return;
			if (storageKind == _StorageKind.Directory)
				throw new BadStorageRequest(string.Format(UserMessages.DeleteErrorDeletedDirectoryAsFile, path));
			_data.Remove(path);
		}

		public void MoveFileNeedsToBeMadeDelayStart(FsPath src, FsPath dest)
		{
			var srcKind = _GetStorage(src)
				.Kind;
			if (srcKind == _StorageKind.Missing)
				throw new BadStorageRequest(string.Format(UserMessages.MoveErrorMissingSource, src._Absolute, dest._Absolute));
			if (srcKind == _StorageKind.Directory)
				throw new BadStorageRequest(string.Format(UserMessages.MoveErrorMovedDirectoryAsFile, src._Absolute, dest._Absolute));
			if (_GetStorage(dest)
				.Kind != _StorageKind.Missing)
				throw new BadStorageRequest(string.Format(UserMessages.MoveErrorDestinationBlocked, src._Absolute, dest._Absolute));
			CreateDir(dest.Parent)
				.RunSynchronouslyAsCheapHackUntilIFixScheduling();
			_MoveItemImpl(src, dest);
		}

		public void MoveDirNeedsToBeMadeDelayStart(FsPath src, FsPath dest)
		{
			var srcKind = _GetStorage(src)
				.Kind;
			var destKind = _GetStorage(dest)
				.Kind;
			if (srcKind == _StorageKind.File)
				throw new BadStorageRequest(string.Format(UserMessages.MoveErrorMovedFileAsDirectory, src._Absolute));
			if (srcKind == _StorageKind.Missing)
				throw new BadStorageRequest(string.Format(UserMessages.MoveErrorMissingSource, src._Absolute));
			if (destKind != _StorageKind.Missing)
				throw new BadStorageRequest(string.Format(UserMessages.MoveErrorDestinationBlocked, src._Absolute, dest._Absolute));
			var itemsToMove = _ItemsInScopeOfDirectory(src);
			itemsToMove.Each(item => _MoveItemImpl(item.Key, item.Key._ReplaceAncestor(src, dest, item.Value.Kind == _StorageKind.Directory)));
		}

		[NotNull]
		private List<KeyValuePair<FsPath, _Node>> _ItemsInScopeOfDirectory([NotNull] FsPath path)
		{
			return _data.Where(item => path.IsAncestorOf(item.Key, item.Value.Kind == _StorageKind.Directory))
				.ToList();
		}

		public IEnumerable<FsPath> FindFilesNeedsToBeMadeDelayStart(FsPath path, string searchPattern)
		{
			var patternExtensionDelimiter = searchPattern.LastIndexOf('.');
			if (patternExtensionDelimiter < 0)
				return Enumerable.Empty<FsPath>();
			var patternBaseName = searchPattern.Substring(0, patternExtensionDelimiter);
			var patternExtension = searchPattern.Substring(patternExtensionDelimiter + 1);
			return _data.Where(
				item =>
					item.Value.Kind == _StorageKind.File && item.Key.Parent == path && _PatternMatches(patternBaseName, patternExtension, Path.GetFileName(item.Key._Absolute)))
				.Select(item => item.Key);
		}

		private void _MoveItemImpl([NotNull] FsPath src, [NotNull] FsPath dest)
		{
			_data[dest] = _data[src];
			_data.Remove(src);
		}

		private static bool _PatternMatches([NotNull] string patternBaseName, [NotNull] string patternExtension, [NotNull] string fileName)
		{
			var extension = Path.GetExtension(fileName);
			extension = string.IsNullOrEmpty(extension) ? string.Empty : extension.Substring(1);
			var baseName = Path.GetFileNameWithoutExtension(fileName);
			return (patternBaseName == "*" || baseName == patternBaseName) && (patternExtension == "*" || extension == patternExtension);
		}

// ReSharper disable once UnusedParameter.Local
		private static void _ValidateStorage([NotNull] FsPath path, [NotNull] _Node storage)
		{
			if (storage.Kind == _StorageKind.Missing)
				throw new BadStorageRequest(string.Format(UserMessages.ReadErrorFileNotFound, path));
			if (storage.Kind == _StorageKind.Directory)
				throw new BadStorageRequest(string.Format(UserMessages.ReadErrorPathIsDirectory, path));
		}

		[NotNull]
		private _Node _GetStorage([NotNull] FsPath path)
		{
			if (path.IsRoot)
				return new _Node(_StorageKind.Directory);
			_Node storage;
			return !_data.TryGetValue(path, out storage) ? new _Node(_StorageKind.Missing) : storage;
		}

		private class _Node
		{
			public _Node(_StorageKind storageKind)
			{
				Kind = storageKind;
				RawContents = new byte[0];
			}

			public _StorageKind Kind { get; private set; }

			[NotNull]
			public byte[] RawContents { get; set; }
		}

		private enum _StorageKind
		{
			Directory,
			File,
			Missing,
		}
	}
}
