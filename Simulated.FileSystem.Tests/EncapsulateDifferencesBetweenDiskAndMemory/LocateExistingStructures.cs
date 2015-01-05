﻿// SimulatableAPI
// File: LocateExistingStructures.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using JetBrains.Annotations;
using NUnit.Framework;
using Simulated.Tests.zzTestHelpers;
using Simulated._Fs;

namespace Simulated.Tests.EncapsulateDifferencesBetweenDiskAndMemory
{
	[TestFixture]
	public abstract class LocateExistingStructures : DiskTestBase
	{
		[NotNull,Test]
		[TestCase("matches.*", new[] {"matches.txt", "matches.jpg"})]
		[TestCase("*.*", new[] {"matches.txt", "matches.jpg", "no_match.txt"})]
		[TestCase("*.txt", new[] {"matches.txt", "no_match.txt"})]
		[TestCase("matches.txt", new[] {"matches.txt"})]
		public async Task FileMatchingShouldMatchStarPatterns([NotNull] string searchPattern, [NotNull] string[] expectedMatches)
		{
			await TestSubject.OverwriteNeedsToBeMadeDelayStart(BaseFolder/"matches.txt", ArbitraryFileContents);
			await TestSubject.OverwriteNeedsToBeMadeDelayStart(BaseFolder/"matches.jpg", ArbitraryFileContents);
			await TestSubject.OverwriteNeedsToBeMadeDelayStart(BaseFolder/"no_match.txt", ArbitraryFileContents);
			TestSubject.FindFilesNeedsToBeMadeDelayStart(BaseFolder, searchPattern)
				.Should()
				.BeEquivalentTo(expectedMatches.Select(m => BaseFolder/m));
		}
	}

	public class LocateExistingStructuresDiskFs : LocateExistingStructures
	{
		internal override _IFsDisk _MakeTestSubject()
		{
			return new _DiskReal();
		}
	}

	public class LocateExistingStructuresMemoryFs : LocateExistingStructures
	{
		internal override _IFsDisk _MakeTestSubject()
		{
			return new _DiskSimulated();
		}
	}
}
