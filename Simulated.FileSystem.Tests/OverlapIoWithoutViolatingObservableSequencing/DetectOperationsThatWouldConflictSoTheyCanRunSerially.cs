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
		[NotNull] private const string _ = "";
		[NotNull] private static readonly FsPath ArbitraryPath = FsPath.TempFolder/"A";
		[NotNull] private static readonly FsPath AnyOtherPath = FsPath.TempFolder/"B";
		[NotNull] private static readonly FsPath AThirdPath = FsPath.TempFolder/"C";
		[NotNull] private static readonly _DiskChange EmptySetOfWork = null;

		[Test]
		[TestCaseSource("OperationConflictsSameTarget")]
		public void OpsWithSameTarget_Should_ConflictCorrectly(bool expected, [NotNull] object op1, [NotNull] object op2)
		{
			((_SingleDiskChange) op1).ConflictsWith((_SingleDiskChange) op2)
				.Should()
				.Be(expected);
		}

		[Test]
		[TestCaseSource("OperationConflictsDifferentTarget")]
		public void OpsWithDifferentTargets_Should_ConflictCorrectly(bool expected, [NotNull] object op1, [NotNull] object op2)
		{
			((_DiskChange) op1).ConflictsWith((_DiskChange) op2)
				.Should()
				.Be(expected);
		}

		[Test]
		public void OpsThatDoNotConflict_Should_BeScheduledTogetherInOrder()
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
		public void Work_Should_OnlyBeScheduledOnce()
		{
			var testSubject = new _OperationBacklog();
			testSubject.MonitorEvents();
			var first = new _TestOperation(1);
			testSubject.Enqueue(first);
			testSubject.FinishedSomeWork(EmptySetOfWork);
			testSubject.FinishedSomeWork(EmptySetOfWork);
			testSubject.ScheduledWork()
				.Should()
				.Equal(new object[] {new _ParallelSafeWorkSet(new[] {first})});
		}

		[Test]
		public void WhenWorkItemsConflict_Should_ChooseFirstItem()
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
		public void WhenFirstItemConflictsWithEverything_Should_ChooseFirstItem()
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
		public void WhenLaterItemsDoNotConflictWithAnything_Should_ChooseThem()
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
		public void WhenLaterItemsConflictWithAnyPriorItem_Should_NotChooseThem()
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
		public void WhenWorkIsFinished_Should_ChooseFromRemainingWork()
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
				_Op.WriteFile(firstTarget, _),
				_Op.FindFiles(firstTarget, _),
				_Op.FileExists(firstTarget),
				_Op.DirectoryExists(firstTarget),
			};
			var rhsOps = new object[]
			{
				_Op.DeleteDirectory(secondTarget),
				_Op.CreateDirectory(secondTarget),
				_Op.ReadFile(secondTarget),
				_Op.WriteFile(secondTarget, _),
				_Op.FindFiles(secondTarget, _),
				_Op.FileExists(secondTarget),
				_Op.DirectoryExists(secondTarget)
			};
			return (from first in Enumerable.Range(0, lhsOps.Length)
				from second in Enumerable.Range(0, rhsOps.Length)
				select new[] {data[first][second] == 'X', lhsOps[first], rhsOps[second]}).ToArray();
		}

		[NotNull]
		private static _TestOperation[] _MakeWorkItems(int howMany)
		{
			var ops = Enumerable.Range(0, howMany)
				.Select(name => new _TestOperation(name))
				.ToArray();
			return ops;
		}
	}
}
