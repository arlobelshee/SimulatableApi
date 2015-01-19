// SimulatableAPI
// File: CanFindEntryPoints.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using JetBrains.Annotations;
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
			FsPath.TempFolder.ToString()
				.Should()
				.Be("{Temp folder}");
		}

		[Test]
		public void TempFolderInternalAccessShouldGiveFullPathInformation()
		{
			FsPath.TempFolder._Absolute.Should()
				.Be(Path.GetTempPath()
					.TrimEnd('\\'));
		}

		[NotNull]
		[Test]
		public async Task TempFolderShouldInitiallyExist()
		{
			(await TestSubject.TempDirectory.Exists).Should()
				.BeTrue();
		}
	}
}
