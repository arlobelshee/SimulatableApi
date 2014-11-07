using System.Collections.ObjectModel;
using System.Linq;
using JetBrains.Annotations;

namespace Simulated._Fs
{
	internal class _MultipleDiskChanges : _DiskChangeKind
	{
		[NotNull] private readonly _DiskChangeKind[] _changes;

		public _MultipleDiskChanges([NotNull] params _DiskChangeKind[] changes)
		{
			_changes = changes;
		}

		[NotNull]
		public ReadOnlyCollection<_DiskChangeKind> Changes
		{
			get { return _changes.ToList().AsReadOnly(); }
		}

		public override bool ConflictsWith(_DiskChangeKind op2)
		{
			return _changes.Any(op2.ConflictsWith);
		}
	}
}