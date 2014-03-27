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
		private const string Contents = "";
		private static readonly FsPath ArbitraryPath = FsPath.TempFolder/"A";
		private static readonly FsPath AnyOtherPath = FsPath.TempFolder/"B";

		[Test]
		[TestCaseSource("OperationConflictsSameTarget")]
		public void OpsWithSameTarget_Should_ConflictCorrectly(bool expected, [NotNull] object op1, [NotNull] object op2)
		{
			((_Op) op1).ConflictsWith((_Op) op2)
				.Should()
				.Be(expected);
		}

		[Test]
		[TestCaseSource("OperationConflictsDifferentTarget")]
		public void OpsWithDifferentTargets_Should_ConflictCorrectly(bool expected, [NotNull] object op1, [NotNull] object op2)
		{
			((_Op) op1).ConflictsWith((_Op) op2)
				.Should()
				.Be(expected);
		}

		[NotNull]
		public static object[][] OperationConflictsSameTarget
		{
			get { return _ParseIntoCases(@"
 |DCRW
------
D|.XXX
C|X..X
R|X..X
W|XXXX
", ArbitraryPath, ArbitraryPath); }
		}

		[NotNull]
		public static object[][] OperationConflictsDifferentTarget
		{
			get { return _ParseIntoCases(@"
 |DCRW
------
D|.XXX
C|X...
R|X...
W|X...
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
				_Op.WriteFile(firstTarget, Contents)
			};
			var rhsOps = new object[]
			{
				_Op.DeleteDirectory(secondTarget),
				_Op.CreateDirectory(secondTarget),
				_Op.ReadFile(secondTarget),
				_Op.WriteFile(secondTarget, Contents)
			};
			return (from first in Enumerable.Range(0, lhsOps.Length)
				from second in Enumerable.Range(0, rhsOps.Length)
				select new[] {data[first][second] == 'X', lhsOps[first], rhsOps[second]}).ToArray();
		}
	}
}
