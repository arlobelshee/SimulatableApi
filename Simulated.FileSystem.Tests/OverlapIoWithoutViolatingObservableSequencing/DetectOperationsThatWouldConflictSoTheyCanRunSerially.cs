// SimulatableAPI
// File: DetectOperationsThatWouldConflictSoTheyCanRunSerially.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System.Linq;
using FluentAssertions;
using JetBrains.Annotations;
using NUnit.Framework;
using Simulated.Tests.zzTestHelpers;
using Simulated._Fs;

namespace Simulated.Tests.OverlapIoWithoutViolatingObservableSequencing
{
	[TestFixture]
	public class DetectOperationsThatWouldConflictSoTheyCanRunSerially
	{
		[NotNull] private static readonly FsPath ArbitraryPath = FsPath.TempFolder/"A";
		[NotNull] private static readonly FsPath AnyOtherPath = FsPath.TempFolder/"B";
		[NotNull] private static readonly _DiskChange EmptySetOfWork = null;

		[Test]
		[TestCaseSource("OperationConflictsSameTarget")]
		public void OpsWithSameTargetShouldConflictCorrectly(bool expected, [NotNull] object op1, [NotNull] object op2)
		{
			((_SingleDiskChange) op1).ConflictsWith((_SingleDiskChange) op2)
				.Should()
				.Be(expected);
		}

		[Test]
		[TestCaseSource("OperationConflictsDifferentTarget")]
		public void OpsWithDifferentTargetsShouldConflictCorrectly(bool expected, [NotNull] object op1, [NotNull] object op2)
		{
			((_DiskChangeKind) op1).ConflictsWith((_DiskChangeKind) op2)
				.Should()
				.Be(expected);
		}

		[Test]
		public void OpsThatDoNotConflictShouldBeScheduledTogetherInOrder()
		{
			var testSubject = new _OperationBacklog();
			testSubject.MonitorEvents();
			var work = _MakeWorkItems(3);
			testSubject.EnqueueAll(work);
			testSubject.FinishedSomeWork(EmptySetOfWork);
			testSubject.ScheduledWork()
				.Should()
				.Equal(new _ParallelSafeWorkSet(work));
		}

		[Test]
		public void WorkShouldOnlyBeScheduledOnce()
		{
			var testSubject = new _OperationBacklog();
			testSubject.MonitorEvents();
			var first = _MakeWorkItems(1).First();
			testSubject.Enqueue(first);
			testSubject.FinishedSomeWork(EmptySetOfWork);
			testSubject.FinishedSomeWork(EmptySetOfWork);
			testSubject.ScheduledWork()
				.Should()
				.Equal(new object[] {new _ParallelSafeWorkSet(new[] {first})});
		}

		[Test]
		public void WhenWorkItemsConflictShouldChooseFirstItem()
		{
			var testSubject = new _OperationBacklog();
			testSubject.MonitorEvents();
			var work = _MakeWorkItems(2);
			work.CreateConflict(0, 1);
			testSubject.EnqueueAll(work);
			testSubject.FinishedSomeWork(EmptySetOfWork);
			testSubject.ScheduledWork()
				.Should()
				.Equal(new object[] {new _ParallelSafeWorkSet(new[] {work[0]})});
		}

		[Test]
		public void WhenFirstItemConflictsWithEverythingShouldChooseFirstItem()
		{
			var testSubject = new _OperationBacklog();
			testSubject.MonitorEvents();
			var work = _MakeWorkItems(3);
			work.CreateConflict(0, 1);
			work.CreateConflict(0, 2);
			testSubject.EnqueueAll(work);
			testSubject.FinishedSomeWork(EmptySetOfWork);
			testSubject.ScheduledWork()
				.Should()
				.Equal(new object[] {new _ParallelSafeWorkSet(new[] {work[0]})});
		}

		[Test]
		public void WhenLaterItemsDoNotConflictWithAnythingShouldChooseThem()
		{
			var testSubject = new _OperationBacklog();
			testSubject.MonitorEvents();
			var work = _MakeWorkItems(3);
			work.CreateConflict(0, 1);
			testSubject.EnqueueAll(work);
			testSubject.FinishedSomeWork(EmptySetOfWork);
			testSubject.ScheduledWork()
				.Should()
				.Equal(new object[] {new _ParallelSafeWorkSet(new[] {work[0], work[2]})});
		}

		[Test]
		public void WhenLaterItemsConflictWithAnyPriorItemShouldNotChooseThem()
		{
			var testSubject = new _OperationBacklog();
			testSubject.MonitorEvents();
			var work = _MakeWorkItems(3);
			work.CreateConflict(0, 1);
			work.CreateConflict(1, 2);
			testSubject.EnqueueAll(work);
			testSubject.FinishedSomeWork(EmptySetOfWork);
			testSubject.ScheduledWork()
				.Should()
				.Equal(new object[] {new _ParallelSafeWorkSet(new[] {work[0]})});
		}

		[Test]
		public void WhenWorkIsFinishedShouldChooseFromRemainingWork()
		{
			var testSubject = new _OperationBacklog();
			testSubject.MonitorEvents();
			var work = _MakeWorkItems(3);
			work.CreateConflict(0, 1);
			testSubject.EnqueueAll(work);
			testSubject.FinishedSomeWork(work[0]);
			testSubject.ScheduledWork()
				.Should()
				.Equal(new object[] {new _ParallelSafeWorkSet(new[] {work[1], work[2]})});
		}

		[Test]
		public void FileDeleteShouldConflictTheSameAsFileWrite()
		{
			_Op.DeleteFile(ArbitraryPath)
				.Should()
				.Be(_Op.WriteFile(ArbitraryPath));
		}

		[Test]
		public void MultipleChangesShouldConflictIfAnyOneIncludedChangeConflicts()
		{
			var benignChange = _Op.FileExists(ArbitraryPath);
			var someChange = _Op.DirectoryExists(ArbitraryPath);
			var changeWhichConflicts = _Op.CreateDirectory(ArbitraryPath);
			var firstChange = new _MultipleDiskChanges(benignChange, someChange);
			var secondChange = new _MultipleDiskChanges(benignChange, changeWhichConflicts);

			firstChange.ConflictsWith(secondChange)
				.Should()
				.Be(true);
		}

		[Test]
		public void MultipleChangesShouldNotConflictIfAllIncludedChangesDontConflict()
		{
			var benignChange = _Op.FileExists(ArbitraryPath);
			var someChange = _Op.DirectoryExists(ArbitraryPath);
			var changeWhichDoesNotConflict = _Op.DirectoryExists(ArbitraryPath);
			var firstChange = new _MultipleDiskChanges(benignChange, someChange);
			var secondChange = new _MultipleDiskChanges(benignChange, changeWhichDoesNotConflict);

			firstChange.ConflictsWith(secondChange)
				.Should()
				.Be(false);
		}

		[Test]
		public void CanCheckSingleOpAgainstMultipleForConflictsInEitherOrder()
		{
			var benignChange = _Op.FileExists(ArbitraryPath);
			var someChange = _Op.DirectoryExists(ArbitraryPath);
			var changeWhichDoesNotConflict = _Op.DirectoryExists(ArbitraryPath);
			var changeWhichConflicts = _Op.CreateDirectory(ArbitraryPath);
			var combinedChange = new _MultipleDiskChanges(benignChange, someChange);

			combinedChange.ConflictsWith(changeWhichConflicts)
				.Should()
				.Be(true);
			combinedChange.ConflictsWith(changeWhichDoesNotConflict)
				.Should()
				.Be(false);
			changeWhichConflicts.ConflictsWith(combinedChange)
				.Should()
				.Be(true);
			changeWhichDoesNotConflict.ConflictsWith(combinedChange)
				.Should()
				.Be(false);
		}

		[Test]
		public void DirectoryMoveShouldBeACreatePlusADelete()
		{
			var moveDirectory = (_MultipleDiskChanges) _Op.MoveDirectory(ArbitraryPath, AnyOtherPath);
			moveDirectory.Changes.Should()
				.BeEquivalentTo(_Op.DeleteDirectory(ArbitraryPath), _Op.CreateDirectory(AnyOtherPath));
		}

		[Test]
		public void FileMoveShouldBeACreatePlusADelete()
		{
			var moveDirectory = (_MultipleDiskChanges) _Op.MoveFile(ArbitraryPath, AnyOtherPath);
			moveDirectory.Changes.Should()
				.BeEquivalentTo(_Op.DeleteFile(ArbitraryPath), _Op.WriteFile(AnyOtherPath));
		}

		[NotNull]
		public static object[][] OperationConflictsSameTarget
		{
			get { return _ParseIntoCases(@"
 |DCRWFeE
---------
D|.XXXXXX
C|X..X..X
R|X..X...
W|XXXXXXX
F|X..X...
e|X..X...
E|XX.X...
", ArbitraryPath, ArbitraryPath); }
		}

		[NotNull]
		public static object[][] OperationConflictsDifferentTarget
		{
			get { return _ParseIntoCases(@"
 |DCRWFeE
---------
D|.XXXXXX
C|X.....X
R|X......
W|X...X.X
F|X..X...
e|X......
E|XX.X...
", ArbitraryPath, AnyOtherPath); }
		}

		[NotNull]
		private static object[][] _ParseIntoCases([NotNull] string conflicts, [NotNull] FsPath firstTarget, [NotNull] FsPath secondTarget)
		{
			var data = conflicts.Trim()
				.Split('\n')
				.Skip(2)
				.Select(s => s.Trim()
					.Substring(2))
				.ToArray();
			var lhsOps = new object[]
			{
				_Op.DeleteDirectory(firstTarget),
				_Op.CreateDirectory(firstTarget),
				_Op.ReadFile(firstTarget),
				_Op.WriteFile(firstTarget),
				_Op.FindFiles(firstTarget),
				_Op.FileExists(firstTarget),
				_Op.DirectoryExists(firstTarget)
			};
			var rhsOps = new object[]
			{
				_Op.DeleteDirectory(secondTarget),
				_Op.CreateDirectory(secondTarget),
				_Op.ReadFile(secondTarget),
				_Op.WriteFile(secondTarget),
				_Op.FindFiles(secondTarget),
				_Op.FileExists(secondTarget),
				_Op.DirectoryExists(secondTarget)
			};
			return (from first in Enumerable.Range(0, lhsOps.Length)
				from second in Enumerable.Range(0, rhsOps.Length)
				select new[] {data[first][second] == 'X', lhsOps[first], rhsOps[second]}).ToArray();
		}

		[NotNull]
		private static _DiskChange[] _MakeWorkItems(int howMany)
		{
			var ops = Enumerable.Range(0, howMany)
				.Select(name => new _DiskChange(new _TestOperation(name)))
				.ToArray();
			return ops;
		}
	}
}
