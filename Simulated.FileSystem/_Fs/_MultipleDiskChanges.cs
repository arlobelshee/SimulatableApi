using System.Collections.ObjectModel;
using System.Linq;
using JetBrains.Annotations;

namespace Simulated._Fs
{
	internal class _MultipleDiskChanges : _DiskChange
	{
		[NotNull] private readonly _DiskChange[] _changes;

		public _MultipleDiskChanges([NotNull] params _DiskChange[] changes)
		{
			_changes = changes;
		}

		[NotNull]
		public ReadOnlyCollection<_DiskChange> Changes
		{
			get { return _changes.ToList().AsReadOnly(); }
		}

		public override bool ConflictsWith(_DiskChange op2)
		{
			return _changes.Any(op2.ConflictsWith);
		}
	}
}