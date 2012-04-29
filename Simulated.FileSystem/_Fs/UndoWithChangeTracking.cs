using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Simulated._Fs
{
	internal class _UndoWithChangeTracking : _Undo
	{
		[NotNull] private readonly FileSystem _fileSystem;
		[NotNull] private readonly List<Action> _undoActions = new List<Action>();

		public _UndoWithChangeTracking(FileSystem fileSystem)
		{
			_fileSystem = fileSystem;
		}

		public override bool IsTrackingChanges
		{
			get { return true; }
		}

		public override void RevertAll()
		{
			Enumerable.Reverse(_undoActions).Each(undo => undo());
			_undoActions.Clear();
		}

		public override void CreatedDirectory(FsPath path)
		{
			_undoActions.Add(() => _fileSystem._Disk.DeleteDir(path));
		}

		public override void Overwrote(FsPath path)
		{
			if (!_fileSystem._Disk.FileExists(path))
			{
				_undoActions.Add(() => _fileSystem._Disk.DeleteFile(path));
				return;
			}
			var randomFileName = FsPath.TempFolder/Guid.NewGuid().ToString("N");
			_fileSystem._Disk.MoveFile(path, randomFileName);
			_undoActions.Add(() =>
			{
				_fileSystem._Disk.DeleteFile(path);
				_fileSystem._Disk.MoveFile(randomFileName, path);
			});
		}
	}
}
