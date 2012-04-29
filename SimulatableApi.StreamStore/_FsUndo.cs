﻿using JetBrains.Annotations;

namespace SimulatableApi.StreamStore
{
	internal class _FsUndo
	{
		public virtual bool IsTrackingChanges
		{
			get { return false; }
		}

		public virtual void RevertAll() {}
		public virtual void CreatedDirectory([NotNull] FsPath path) {}
		public virtual void Overwrote([NotNull] FsPath path) {}
	}
}