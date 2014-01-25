// SimulatableAPI
// File: _DiskSimulated.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace Simulated._Fs
{
	internal class _DiskSimulated : _IFsDisk
	{
		private readonly Dictionary<FsPath, _Node> _data = new Dictionary<FsPath, _Node>();
		private static readonly Encoding DefaultEncoding = Encoding.UTF8;

		public bool DirExists(FsPath path)
		{
			return _GetStorage(path)
				.Kind == _StorageKind.Directory;
		}

		public bool FileExists(FsPath path)
		{
			return _GetStorage(path)
				.Kind == _StorageKind.File;
		}

		public string TextContents(FsPath path)
		{
			var storage = _GetStorage(path);
			_ValidateStorage(path, storage);
			return DefaultEncoding.GetString(storage.RawContents);
		}

		public byte[] RawContents(FsPath path)
		{
			var storage = _GetStorage(path);
			_ValidateStorage(path, storage);
			return storage.RawContents;
		}

		public void CreateDir(FsPath path)
		{
			while (true)
			{
				_data[path] = new _Node(_StorageKind.Directory);
				if (path.IsRoot)
					return;
				path = path.Parent;
			}
		}

		public void Overwrite(FsPath path, string newContents)
		{
			CreateDir(path.Parent);
			_data[path] = new _Node(_StorageKind.File)
			{
				RawContents = DefaultEncoding.GetBytes(newContents)
			};
		}

		public void Overwrite(FsPath path, byte[] newContents)
		{
			CreateDir(path.Parent);
			_data[path] = new _Node(_StorageKind.File)
			{
				RawContents = newContents
			};
		}

		public void DeleteDir(FsPath path)
		{
			var storageKind = _GetStorage(path)
				.Kind;
			if (storageKind != _StorageKind.Directory)
				return;
			var toDelete = _ItemsInScopeOfDirectory(path);
			toDelete.Each(p => _data.Remove(p.Key));
		}

		public void DeleteFile(FsPath path)
		{
			var storageKind = _GetStorage(path)
				.Kind;
			if (storageKind == _StorageKind.Missing)
				return;
			if (storageKind == _StorageKind.Directory)
				throw new UnauthorizedAccessException(string.Format("Access to the path '{0}' is denied.", path.Absolute));
			_data.Remove(path);
		}

		public void MoveFile(FsPath src, FsPath dest)
		{
			if (_GetStorage(src)
				.Kind != _StorageKind.File)
				throw new FileNotFoundException(string.Format("Could not find file '{0}'.", src.Absolute));
			if (_GetStorage(dest)
				.Kind != _StorageKind.Missing)
				throw new BadStorageRequest(string.Format("Cannot move '{0}' to '{1}' because there is already something at the destination.", src.Absolute, dest.Absolute));
			CreateDir(dest.Parent);
			_MoveItemImpl(src, dest);
		}

		public void MoveDir(FsPath src, FsPath dest)
		{
			var srcKind = _GetStorage(src)
				.Kind;
			if (srcKind == _StorageKind.File)
				throw new UnauthorizedAccessException(string.Format("Cannot move the directory '{0}' because it is a file.", src.Absolute));
			if (srcKind == _StorageKind.Missing)
				throw new BadStorageRequest(string.Format("Cannot move '{0}' because it does not exist.", src.Absolute));
			if (_GetStorage(dest)
				.Kind != _StorageKind.Missing)
				throw new BadStorageRequest(string.Format("Cannot move '{0}' to '{1}' because there is already something at the destination.", src.Absolute, dest.Absolute));
			var itemsToMove = _ItemsInScopeOfDirectory(src);
			itemsToMove.Each(item => _MoveItemImpl(item.Key, item.Key.ReplaceAncestor(src, dest, item.Value.Kind == _StorageKind.Directory)));
		}

		[NotNull]
		private List<KeyValuePair<FsPath, _Node>> _ItemsInScopeOfDirectory([NotNull] FsPath path)
		{
			return _data.Where(item => path.IsAncestorOf(item.Key, item.Value.Kind == _StorageKind.Directory))
				.ToList();
		}

		public IEnumerable<FsPath> FindFiles(FsPath path, string searchPattern)
		{
			var patternExtensionDelimiter = searchPattern.LastIndexOf('.');
			if (patternExtensionDelimiter < 0)
				return Enumerable.Empty<FsPath>();
			var patternBaseName = searchPattern.Substring(0, patternExtensionDelimiter);
			var patternExtension = searchPattern.Substring(patternExtensionDelimiter + 1);
			return _data.Where(
				item =>
					item.Value.Kind == _StorageKind.File && item.Key.Parent == path && _PatternMatches(patternBaseName, patternExtension, Path.GetFileName(item.Key.Absolute)))
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
