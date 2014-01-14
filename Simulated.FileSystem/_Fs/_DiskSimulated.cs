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
	internal class _DiskSimulated : _StorageSink
	{
		[NotNull] private readonly Dictionary<FsPath, _Node> _data = new Dictionary<FsPath, _Node>();
		[NotNull] private static readonly Encoding DefaultEncoding = Encoding.UTF8;

		public override Task<bool> DirExists(FsPath path)
		{
			return (_GetStorage(path)
				.Kind == _StorageKind.Directory).AsImmediateTask();
		}

		public override Task<bool> FileExists(FsPath path)
		{
			return (_GetStorage(path)
				.Kind == _StorageKind.File).AsImmediateTask();
		}

		public override Task<string> TextContents(FsPath path)
		{
			var storage = _GetStorage(path);
			_ValidateStorage(path, storage);
			return DefaultEncoding.GetString(storage.RawContents)
				.AsImmediateTask();
		}

		public override Task<byte[]> RawContents(FsPath path)
		{
			var storage = _GetStorage(path);
			_ValidateStorage(path, storage);
			return storage.RawContents.AsImmediateTask();
		}

		public override Task CreateDir(FsPath path)
		{
			while (true)
			{
				_data[path] = new _Node(_StorageKind.Directory);
				if (path.IsRoot)
					return CompletedTask;
				path = path.Parent;
			}
		}

		public override Task Overwrite(FsPath path, string newContents)
		{
			_data[path] = new _Node(_StorageKind.File)
			{
				RawContents = DefaultEncoding.GetBytes(newContents)
			};
			return CompletedTask;
		}

		public override Task Overwrite(FsPath path, byte[] newContents)
		{
			_data[path] = new _Node(_StorageKind.File)
			{
				RawContents = newContents
			};
			return CompletedTask;
		}

		public override Task DeleteDir(FsPath path)
		{
			var storageKind = _GetStorage(path)
				.Kind;
			if (storageKind == _StorageKind.Missing)
				return CompletedTask;
			if (storageKind == _StorageKind.File)
				throw new ArgumentException("path", string.Format("Path {0} was a file, and you attempted to delete a directory.", path.Absolute));
			var toDelete = _ItemsInScopeOfDirectory(path);
			toDelete.Each(p => _data.Remove(p.Key));
			return CompletedTask;
		}

		public override Task DeleteFile(FsPath path)
		{
			var storageKind = _GetStorage(path)
				.Kind;
			if (storageKind == _StorageKind.Missing)
				return CompletedTask;
			if (storageKind == _StorageKind.Directory)
				throw new ArgumentException("path", string.Format("Path {0} was a directory, and you attempted to delete a file.", path.Absolute));
			_data.Remove(path);
			return CompletedTask;
		}

		public override Task MoveFile(FsPath src, FsPath dest)
		{
			if (_GetStorage(src)
				.Kind != _StorageKind.File)
				throw new ArgumentException("path", string.Format("Attempted to move file {0}, which is not a file.", src.Absolute));
			if (_GetStorage(dest)
				.Kind != _StorageKind.Missing)
				throw new ArgumentException("path", string.Format("Attempted to move file to destination {0}, which already exists.", dest.Absolute));
			_MoveItemImpl(src, dest);
			return CompletedTask;
		}

		public override Task MoveDir(FsPath src, FsPath dest)
		{
			if (_GetStorage(src)
				.Kind != _StorageKind.Directory)
				throw new ArgumentException("path", string.Format("Attempted to move directory {0}, which is not a directory.", src.Absolute));
			if (_GetStorage(dest)
				.Kind != _StorageKind.Missing)
				throw new ArgumentException("path", string.Format("Attempted to move directory to destination {0}, which already exists.", dest.Absolute));
			var itemsToMove = _ItemsInScopeOfDirectory(src);
			itemsToMove.Each(item => _MoveItemImpl(item.Key, item.Key.ReplaceAncestor(src, dest, item.Value.Kind == _StorageKind.Directory)));
			return CompletedTask;
		}

		public override Task<IEnumerable<FsPath>> FindFiles(FsPath path, string searchPattern)
		{
			var patternExtensionDelimiter = searchPattern.LastIndexOf('.');
			if (patternExtensionDelimiter < 0)
				return Enumerable.Empty<FsPath>()
					.AsImmediateTask();
			var patternBaseName = searchPattern.Substring(0, patternExtensionDelimiter);
			var patternExtension = searchPattern.Substring(patternExtensionDelimiter + 1);
			return
				_data.Where(
					item =>
						item.Value.Kind == _StorageKind.File && item.Key.Parent == path && _PatternMatches(patternBaseName, patternExtension, Path.GetFileName(item.Key.Absolute)))
					.Select(item => item.Key)
					.AsImmediateTask();
		}

		private void _MoveItemImpl([NotNull] FsPath src, [NotNull] FsPath dest)
		{
			_data[dest] = _data[src];
			_data.Remove(src);
		}

		private bool _PatternMatches([NotNull] string patternBaseName, [NotNull] string patternExtension, [NotNull] string fileName)
		{
			var extension = Path.GetExtension(fileName);
			extension = string.IsNullOrEmpty(extension) ? string.Empty : extension.Substring(1);
			var baseName = Path.GetFileNameWithoutExtension(fileName);
			return (patternBaseName == "*" || baseName == patternBaseName) && (patternExtension == "*" || extension == patternExtension);
		}

// ReSharper disable once UnusedParameter.Local
		private void _ValidateStorage([NotNull] FsPath path, [NotNull] _Node storage)
		{
			if (storage.Kind == _StorageKind.Missing)
				throw new FileNotFoundException(string.Format("Could not find file '{0}'.", path.Absolute), path.Absolute);
			if (storage.Kind == _StorageKind.Directory)
				throw new UnauthorizedAccessException(string.Format("Access to the path '{0}' is denied.", path.Absolute));
		}

		[NotNull]
		private _Node _GetStorage([NotNull] FsPath path)
		{
			if (path.IsRoot)
				return new _Node(_StorageKind.Directory);
			_Node storage;
			return !_data.TryGetValue(path, out storage) ? new _Node(_StorageKind.Missing) : storage;
		}

		[NotNull]
		private List<KeyValuePair<FsPath, _Node>> _ItemsInScopeOfDirectory([NotNull] FsPath path)
		{
			return _data.Where(item => path.IsAncestorOf(item.Key, item.Value.Kind == _StorageKind.Directory))
				.ToList();
		}

		private class _Node
		{
			public _Node(_StorageKind storageKind)
			{
				Kind = storageKind;
				RawContents = new byte[0];
			}

			public _StorageKind Kind { get; private set; }
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
