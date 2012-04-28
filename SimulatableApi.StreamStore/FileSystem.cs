using System;
using System.IO;
using JetBrains.Annotations;

namespace SimulatableApi.StreamStore
{
	/// <summary>
	/// Represents a view on the file system. The underlying store could be a real file system or a simulated (in-memory) one. In either case, FileSystem and its
	/// helpers allow a user to interact with an abstraction of this storage.
	/// 
	/// FileSystem also support change tracking. If you call EnableRevertToHere, then you will begin tracking changes from that point. At any point,
	/// you can dispose the FileSystem view or call RevertAllChanges to revert to the last save point. You can also call CommitChanges to commit
	/// all pending changes.
	/// 
	/// Changes are written to the disk as they occcur, so in case of an application crash, changes will be saved, not rolled back.
	/// 
	/// Each FileSystem instance is its own view of the storage, with its own save point. Call Clone to create another view with the same underlying storage but
	/// an independent save point.
	/// </summary>
	public class FileSystem : IDisposable
	{
		private FileSystem([NotNull] IFsDisk disk)
		{
			_Changes = new _FsUndo();
			_Disk = disk;
		}

		public void Dispose()
		{
			RevertAllChanges();
		}

		[NotNull]
		internal IFsDisk _Disk { get; private set; }

		[NotNull]
		internal _FsUndo _Changes { get; private set; }

		[NotNull]
		public FsDirectory TempDirectory
		{
			get { return Directory(Path.GetTempPath()); }
		}

		[NotNull]
		public static FileSystem Real()
		{
			return new FileSystem(new _FsDiskReal());
		}

		[NotNull]
		public static FileSystem Simulated()
		{
			return new FileSystem(new _FsDiskSimulated());
		}

		public void EnableRevertToHere()
		{
			if (!_Changes.IsTrackingChanges)
			{
				_Changes = new _FsUndoWithChangeTracking(this);
			}
		}

		[NotNull]
		public FsDirectory Directory([NotNull] string absolutePath)
		{
			return Directory(new FSPath(absolutePath));
		}

		[NotNull]
		public FsDirectory Directory([NotNull] FSPath path)
		{
			return new FsDirectory(this, path);
		}

		[NotNull]
		public FsFile File([NotNull] string absoluteFilePath)
		{
			return File(new FSPath(absoluteFilePath));
		}

		[NotNull]
		public FsFile File([NotNull] FSPath fileName)
		{
			return new FsFile(this, fileName);
		}

		public void RevertAllChanges()
		{
			_Changes.RevertAll();
		}

		public void CommitChanges()
		{
			if (_Changes.IsTrackingChanges)
			{
				_Changes = new _FsUndo();
			}
		}

		[NotNull]
		public FileSystem Clone()
		{
			return new FileSystem(_Disk);
		}
	}
}
