// SimulatableAPI
// File: CanFindEntryPoints.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System.IO;
using FluentAssertions;
using NUnit.Framework;
using Simulated.Tests.zzTestHelpers;

namespace Simulated.Tests.FileSystemNavigation
{
	public class CanFindEntryPoints : FileSystemTestBase
	{
		[Test]
		public void TempFolderShouldHaveTheCorrectPath()
		{
			TestSubject.TempDirectory.Path.Should()
				.Be(FsPath.TempFolder);
		}

		[Test]
		public void TempFolderShouldNotDisclosePathInformation()
		{
			FsPath.TempFolder.ToString().Should()
				.Be("{Temp folder}");
		}

		[Test]
		public void TempFolderInternalAccessShouldGiveFullPathInformation()
		{
			FsPath.TempFolder._Absolute.Should()
				.Be(Path.GetTempPath().TrimEnd('\\'));
		}

		[Test]
		public void TempFolderShouldInitiallyExist()
		{
			TestSubject.TempDirectory.Exists.Should()
				.BeTrue();
		}
	}
}
