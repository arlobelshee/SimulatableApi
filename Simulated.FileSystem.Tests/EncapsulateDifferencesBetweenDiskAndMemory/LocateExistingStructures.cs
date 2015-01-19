// SimulatableAPI
// File: LocateExistingStructures.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

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
			var filesThatExist = new[] {"matches.txt", "matches.jpg", "no_match.txt"};
			await Task.WhenAll(filesThatExist.Select(f => TestSubject.Overwrite(BaseFolder/f, ArbitraryFileContents))
				.ToArray());
			TestSubject.FindFiles(BaseFolder, searchPattern)
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
