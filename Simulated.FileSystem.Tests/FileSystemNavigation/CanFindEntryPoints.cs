// SimulatableAPI
// File: CanFindEntryPoints.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using FluentAssertions;
using NUnit.Framework;
using Simulated.Tests.zzTestHelpers;

namespace Simulated.Tests.FileSystemNavigation
{
	public abstract class CanFindEntryPoints : FileSystemTestBase
	{
		[Test]
		public void TempFolderShouldHaveTheCorrectPath()
		{
			TestSubject.TempDirectory.Path.Should()
				.Be(FsPath.TempFolder);
		}

		[Test]
		public void TempFolderShouldInitiallyExist()
		{
			TestSubject.TempDirectory.Exists.Should()
				.BeTrue();
		}

		[Test]
		public void ShouldBeAbleToMakeReferenceToAbsolutePath()
		{
			TestSubject.Directory(ArbitraryMissingFolder)
				.Path.Absolute.Should()
				.Be(ArbitraryMissingFolder);
		}

		private const string ArbitraryMissingFolder = @"C:\theroot\folder";
	}
}
