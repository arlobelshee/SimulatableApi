using JetBrains.Annotations;

namespace SimulatableApi
{
	internal class _FsUndo
	{
		public virtual bool IsTrackingChanges
		{
			get { return false; }
		}

		public virtual void RevertAll() {}
		public virtual void CreatedDirectory([NotNull] FSPath path) {}
		public virtual void Overwrote([NotNull] FSPath path) {}
	}
}