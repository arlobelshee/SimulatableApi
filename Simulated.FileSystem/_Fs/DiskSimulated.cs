﻿using System;
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
			_data[path] = new _Node(_StorageKind.File)
			{
				RawContents = DefaultEncoding.GetBytes(newContents)
			};
		}

		public void Overwrite(FsPath path, byte[] newContents)
		{
			_data[path] = new _Node(_StorageKind.File)
			{
				RawContents = newContents
			};
		}

		public void DeleteDir(FsPath path)
		{
			if (_GetStorage(path)
				.Kind == _StorageKind.File)
				throw new ArgumentException("path", string.Format("Path {0} was a file, and you attempted to delete a directory.", path.Absolute));
			_data.Remove(path);
		}

		public void DeleteFile(FsPath path)
		{
			if (_GetStorage(path)
				.Kind == _StorageKind.Directory)
				throw new ArgumentException("path", string.Format("Path {0} was a directory, and you attempted to delete a file.", path.Absolute));
			_data.Remove(path);
		}

		public void MoveFile(FsPath src, FsPath dest)
		{
			if (_GetStorage(src)
				.Kind != _StorageKind.File)
				throw new ArgumentException("path", string.Format("Attempted to move file {0}, which is not a file.", src.Absolute));
			if (_GetStorage(dest)
				.Kind != _StorageKind.Missing)
				throw new ArgumentException("path", string.Format("Attempted to move file to destination {0}, which already exists.", dest.Absolute));
			_MoveItemImpl(src, dest);
		}

		public void MoveDir(FsPath src, FsPath dest)
		{
			if (_GetStorage(src)
				.Kind != _StorageKind.Directory)
				throw new ArgumentException("path", string.Format("Attempted to move directory {0}, which is not a directory.", src.Absolute));
			if (_GetStorage(dest)
				.Kind != _StorageKind.Missing)
				throw new ArgumentException("path", string.Format("Attempted to move directory to destination {0}, which already exists.", dest.Absolute));
			var itemsToMove = _data.Where(item => src.IsAncestorOf(item.Key, item.Value.Kind == _StorageKind.Directory))
				.ToList();
			itemsToMove.Each(item => _MoveItemImpl(item.Key, item.Key.ReplaceAncestor(src, dest, item.Value.Kind == _StorageKind.Directory)));
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

		private void _MoveItemImpl(FsPath src, FsPath dest)
		{
			_data[dest] = _data[src];
			_data.Remove(src);
		}

		private bool _PatternMatches(string patternBaseName, string patternExtension, string fileName)
		{
			var extension = Path.GetExtension(fileName);
			extension = string.IsNullOrEmpty(extension) ? string.Empty : extension.Substring(1);
			var baseName = Path.GetFileNameWithoutExtension(fileName);
			return (patternBaseName == "*" || baseName == patternBaseName) && (patternExtension == "*" || extension == patternExtension);
		}

		private void _ValidateStorage(FsPath path, _Node storage)
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
