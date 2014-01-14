﻿// SimulatableAPI
// File: FileSystem.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Simulated._Fs;

namespace Simulated
{
	/// <summary>
	///    Represents a view on the file system. The underlying store could be a real file system or a simulated (in-memory)
	///    one. In either case, FileSystem and its helpers allow a user to interact with an abstraction of this storage.
	///    FileSystem also supports change tracking. If you call EnableRevertToHere, then you will begin tracking changes from
	///    that point. At any point you can dispose the FileSystem view or call RevertAllChanges to revert to the last save
	///    point. You can also call CommitChanges to commit all pending changes.
	///    Changes are written to the disk as they occcur. In case of an application crash changes will be saved, not rolled
	///    back.
	///    Each FileSystem instance is its own view of the storage with its own save point. Call Clone to create another view
	///    with the same underlying storage but an independent save point.
	/// </summary>
	public class FileSystem : IDisposable
	{
		[NotNull] private readonly AsyncLazy<FsDirectory> _tempDirectory;
		[NotNull] private readonly DirectoryModifier _underlyingStorage;

		private FileSystem([NotNull] _IFsDisk disk)
		{
			_Changes = new _Undo();
			_Disk = disk;
			_underlyingStorage = new DirectoryModifier(this);
			var temp = Directory(Path.GetTempPath());
			_tempDirectory = new AsyncLazy<FsDirectory>(temp.EnsureExists()
				.ContinueWith(result => temp));
		}

		/// <summary>
		///    Rolls back any un-committed changes.
		///    If the <see cref="FileSystem" /> has a save point, then this Dispose will <see cref="RevertAllChanges" />.
		///    If there is no save point or if all changes have been committed with <see cref="CommitChanges" />,
		///    then disposing will have no effect.
		/// </summary>
		public void Dispose()
		{
			RevertAllChanges()
				.Wait();
		}

		[NotNull]
		internal _IFsDisk _Disk { get; private set; }

		[NotNull]
		internal _Undo _Changes { get; private set; }

		/// <summary>
		///    Gets the temp directory.
		/// </summary>
		[NotNull]
		public AsyncLazy<FsDirectory> TempDirectory
		{
			get { return _tempDirectory; }
		}

		[NotNull]
		internal async Task<FsDirectory> _UndoDataCache()
		{
			var undoWithChangeTracking = _Changes as _UndoWithChangeTracking;
			if (undoWithChangeTracking == null)
				return null;
			return Directory(await undoWithChangeTracking.UndoDataCache);
		}

		/// <summary>
		///    Creates a file system instance backed by the actual file system.
		/// </summary>
		/// <returns>an on-disk file system</returns>
		[NotNull]
		public static FileSystem Real()
		{
			return new FileSystem(new _DiskReal());
		}

		/// <summary>
		///    Creates a file system instance that is backed by in-memory storage.
		/// </summary>
		/// <returns>an in-memory file system</returns>
		[NotNull]
		public static FileSystem Simulated()
		{
			return new FileSystem(new _DiskSimulated());
		}

		/// <summary>
		///    Creates a directory instance for an absolute path on this file system.
		///    The directory need not exist in the file system storage. Creating a directory object
		///    will not create the directory.
		///    The path argument must be a full, absolute path (including drive letter).
		/// </summary>
		/// <param name="absolutePath">The path this object should represent.</param>
		/// <exception cref="ArgumentNullException">if the path is null or empty</exception>
		/// <returns>a non-null directory instance</returns>
		[NotNull]
		public FsDirectory Directory([NotNull] string absolutePath)
		{
			return Directory(new FsPath(absolutePath));
		}

		/// <summary>
		///    Creates a directory instance for a path on this file system.
		///    The directory need not exist in the file system storage. Creating a directory object
		///    will not create the directory.
		/// </summary>
		/// <param name="path">The path this object should represent.</param>
		/// <exception cref="ArgumentNullException">if the path is null</exception>
		/// <returns>a non-null directory instance</returns>
		[NotNull]
		public FsDirectory Directory([NotNull] FsPath path)
		{
			return new FsDirectory(this, path, _underlyingStorage);
		}

		/// <summary>
		///    Creates a file instance for a path on this file system.
		///    The file need not exist in the file system storage. Creating a file object
		///    will not create the file.
		///    The path argument must be a full, absolute path (including drive letter).
		/// </summary>
		/// <param name="absoluteFilePath">The path this object should represent.</param>
		/// <exception cref="ArgumentNullException">if the path is null or empty</exception>
		/// <returns>a non-null file instance</returns>
		[NotNull]
		public FsFile File([NotNull] string absoluteFilePath)
		{
			return File(new FsPath(absoluteFilePath));
		}

		/// <summary>
		///    Creates a file instance for a path on this file system.
		///    The file need not exist in the file system storage. Creating a file object
		///    will not create the file.
		/// </summary>
		/// <param name="fileName">The path this object should represent.</param>
		/// <exception cref="ArgumentNullException">if the path is null</exception>
		/// <returns>a non-null file instance</returns>
		[NotNull]
		public FsFile File([NotNull] FsPath fileName)
		{
			return new FsFile(this, fileName);
		}

		/// <summary>
		///    Sets a save point. <see cref="RevertAllChanges" /> will revert to this save point.
		///    Does not override any existing save point. If a save point is already set then
		///    this call has no effect.
		/// </summary>
		public void EnableRevertToHere()
		{
			if (!_Changes.IsTrackingChanges)
			{
				_Changes = new _UndoWithChangeTracking(this);
			}
		}

		/// <summary>
		///    Reverts all changes since the save point.
		/// </summary>
		[NotNull]
		public Task RevertAllChanges()
		{
			if (_Changes.IsTrackingChanges)
			{
				var oldChanges = _Changes;
				_Changes = new _Undo();
				return oldChanges.RevertAll();
			}
			return _Undo.CompletedTask;
		}

		/// <summary>
		///    Commits any pending changes and removes the save point.
		/// </summary>
		[NotNull]
		public Task CommitChanges()
		{
			if (_Changes.IsTrackingChanges)
			{
				var oldChanges = _Changes;
				_Changes = new _Undo();
				return oldChanges.CommitAll();
			}
			return _Undo.CompletedTask;
		}

		/// <summary>
		///    Clones this instance.
		///    The clone shares storage with this instance. Each will see changes made by the other.
		///    The clone has its own save point. Each has its own transaction and can roll back independently.
		///    Each will see the not-yet-committed data from the other. The only difference between the
		///    clones is where they will revert to when they <see cref="RevertAllChanges" />.
		/// </summary>
		/// <returns>a file system with the same storage as this instance</returns>
		[NotNull]
		public FileSystem Clone()
		{
			return new FileSystem(_Disk);
		}
	}
}
