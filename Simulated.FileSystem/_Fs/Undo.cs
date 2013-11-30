using System;
using JetBrains.Annotations;

namespace Simulated._Fs
{
	internal class _Undo
	{
		public _Undo()
		{
			UndoDataCache = FsPath.TempFolder/("UndoData."+Guid.NewGuid().ToString("N"));
		}

		public virtual bool IsTrackingChanges
		{
			get { return false; }
		}

		public FsPath UndoDataCache { get; private set; }

		public virtual void CommitAll() {}
		public virtual void RevertAll() {}
		public virtual void CreatedDirectory([NotNull] FsPath path) {}
		public virtual void Overwrote([NotNull] FsPath path) {}
	}
}
