// SimulatableAPI
// File: DetectOperationsThatWouldConflictSoTheyCanRunSerially.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System.Linq;
using FluentAssertions;
using JetBrains.Annotations;
using NUnit.Framework;
using Simulated._Fs;

namespace Simulated.Tests.OverlapIoWithoutViolatingObservableSequencing
{
	[TestFixture]
	public class DetectOperationsThatWouldConflictSoTheyCanRunSerially
	{
		private const string _ = "";
		private static readonly FsPath ArbitraryPath = FsPath.TempFolder/"A";
		private static readonly FsPath AnyOtherPath = FsPath.TempFolder/"B";

		[Test]
		[TestCaseSource("OperationConflictsSameTarget")]
		public void OpsWithSameTarget_Should_ConflictCorrectly(bool expected, [NotNull] object op1, [NotNull] object op2)
		{
			((_OverlappedLambdaWithKind) op1).ConflictsWith((_OverlappedLambdaWithKind) op2)
				.Should()
				.Be(expected);
		}

		[Test]
		[TestCaseSource("OperationConflictsDifferentTarget")]
		public void OpsWithDifferentTargets_Should_ConflictCorrectly(bool expected, [NotNull] object op1, [NotNull] object op2)
		{
			((_OverlappedOperation) op1).ConflictsWith((_OverlappedOperation) op2)
				.Should()
				.Be(expected);
		}

		[Test]
		public void OpsThatDoNotConflict_Should_BeScheduledTogetherInOrder()
		{
			var testSubject = new _OperationBacklog();
			var first = new _TestOperation();
			var second = new _TestOperation();
			testSubject.Enqueue(first);
			testSubject.Enqueue(second);
			var result = testSubject.DequeueSchedulableWork(new _OverlappedOperation[]{});
			result.Should()
				.Equal(new[] {first, second});
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
				_Op.DirectoryExists(firstTarget)
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
	}

	internal class _TestOperation : _OverlappedOperation
	{
		public override bool ConflictsWith(_OverlappedOperation op2)
		{
			var other = op2 as _TestOperation;
			return false;
		}
	}
}
