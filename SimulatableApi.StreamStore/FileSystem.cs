using System;
using System.IO;
using JetBrains.Annotations;

namespace SimulatableApi.StreamStore
{
	/// <summary>
	/// Represents a view on the file system. The underlying store could be a real file system or a simulated (in-memory) one. In either case, FileSystem and its
	/// helpers allow a user to interact with an abstraction of this storage.
	/// 
	/// FileSystem also supports change tracking. If you call EnableRevertToHere, then you will begin tracking changes from that point. At any point
	/// you can dispose the FileSystem view or call RevertAllChanges to revert to the last save point. You can also call CommitChanges to commit
	/// all pending changes.
	/// 
	/// Changes are written to the disk as they occcur. In case of an application crash changes will be saved, not rolled back.
	/// 
	/// Each FileSystem instance is its own view of the storage with its own save point. Call Clone to create another view with the same underlying storage but
	/// an independent save point.
	/// </summary>
	public class FileSystem : IDisposable
	{
		private FileSystem([NotNull] IFsDisk disk, bool shouldCreateTempFolder)
		{
			_Changes = new _FsUndo();
			_Disk = disk;
			TempDirectory.Create();
		}

		/// <summary>
		/// Rolls back any un-committed changes.
		/// 
		/// If the <see cref="FileSystem"/> has a save point, then this Dispose will <see cref="RevertAllChanges"/>.
		/// If there is no save point or if all changes have been committed with <see cref="CommitChanges"/>,
		/// then disposing will have no effect.
		/// </summary>
		public void Dispose()
		{
			RevertAllChanges();
		}

		[NotNull]
		internal IFsDisk _Disk { get; private set; }

		[NotNull]
		internal _FsUndo _Changes { get; private set; }

		/// <summary>
		/// Gets the temp directory.
		/// </summary>
		[NotNull]
		public FsDirectory TempDirectory
		{
			get { return Directory(Path.GetTempPath()); }
		}

		/// <summary>
		/// Creates a file system instance backed by the actual file system.
		/// </summary>
		/// <returns>an on-disk file system</returns>
		[NotNull]
		public static FileSystem Real()
		{
			return new FileSystem(new _FsDiskReal(), false);
		}

		/// <summary>
		/// Creates a file system instance that is backed by in-memory storage.
		/// </summary>
		/// <returns>an in-memory file system</returns>
		[NotNull]
		public static FileSystem Simulated()
		{
			return new FileSystem(new _FsDiskSimulated(), true);
		}

		/// <summary>
		/// Creates a directory instance for an absolute path on this file system.
		/// 
		/// The directory need not exist in the file system storage. Creating a directory object
		/// will not create the directory.
		/// 
		/// The path argument must be a full, absolute path (including drive letter).
		/// </summary>
		/// <param name="absolutePath">The path this object should represent.</param>
		/// <exception cref="ArgumentNullException">if the path is null or empty</exception>
		/// <returns>a non-null directory instance</returns>
		[NotNull]
		public FsDirectory Directory([NotNull] string absolutePath)
		{
			return Directory(new FSPath(absolutePath));
		}

		/// <summary>
		/// Creates a directory instance for a path on this file system.
		/// 
		/// The directory need not exist in the file system storage. Creating a directory object
		/// will not create the directory.
		/// </summary>
		/// <param name="path">The path this object should represent.</param>
		/// <exception cref="ArgumentNullException">if the path is null</exception>
		/// <returns>a non-null directory instance</returns>
		[NotNull]
		public FsDirectory Directory([NotNull] FSPath path)
		{
			return new FsDirectory(this, path);
		}

		/// <summary>
		/// Creates a file instance for a path on this file system.
		/// 
		/// The file need not exist in the file system storage. Creating a file object
		/// will not create the file.
		/// 
		/// The path argument must be a full, absolute path (including drive letter).
		/// </summary>
		/// <param name="absoluteFilePath">The path this object should represent.</param>
		/// <exception cref="ArgumentNullException">if the path is null or empty</exception>
		/// <returns>a non-null file instance</returns>
		[NotNull]
		public FsFile File([NotNull] string absoluteFilePath)
		{
			return File(new FSPath(absoluteFilePath));
		}

		/// <summary>
		/// Creates a file instance for a path on this file system.
		/// 
		/// The file need not exist in the file system storage. Creating a file object
		/// will not create the file.
		/// </summary>
		/// <param name="fileName">The path this object should represent.</param>
		/// <exception cref="ArgumentNullException">if the path is null</exception>
		/// <returns>a non-null file instance</returns>
		[NotNull]
		public FsFile File([NotNull] FSPath fileName)
		{
			return new FsFile(this, fileName);
		}

		/// <summary>
		/// Sets a save point. <see cref="RevertAllChanges"/> will revert to this save point.
		/// 
		/// Does not override any existing save point. If a save point is already set then
		/// this call has no effect.
		/// </summary>
		public void EnableRevertToHere()
		{
			if (!_Changes.IsTrackingChanges)
			{
				_Changes = new _FsUndoWithChangeTracking(this);
			}
		}

		/// <summary>
		/// Reverts all changes since the save point.
		/// </summary>
		public void RevertAllChanges()
		{
			_Changes.RevertAll();
		}

		/// <summary>
		/// Commits any pending changes and removes the save point.
		/// </summary>
		public void CommitChanges()
		{
			if (_Changes.IsTrackingChanges)
			{
				_Changes = new _FsUndo();
			}
		}

		/// <summary>
		/// Clones this instance.
		/// 
		/// The clone shares storage with this instance. Each will see changes made by the other.
		/// 
		/// The clone has its own save point. Each has its own transaction and can roll back independently.
		/// Each will see the not-yet-committed data from the other. The only difference between the
		/// clones is where they will revert to when they <see cref="RevertAllChanges" />.
		/// </summary>
		/// <returns>a file system with the same storage as this instance</returns>
		[NotNull]
		public FileSystem Clone()
		{
			return new FileSystem(_Disk, false);
		}
	}
}
