// SimulatableAPI
// File: CanIdentifyBasicProperties.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System.Threading.Tasks;
using FluentAssertions;
using JetBrains.Annotations;
using NUnit.Framework;
using Simulated.Tests.zzTestHelpers;

namespace Simulated.Tests.QueryBasicProperties
{
	public class CanIdentifyBasicProperties : FileSystemTestBase
	{
		[Test]
		public void AFileKnowsItsFileNameParts()
		{
			const string fileName = "ArbitraryFile.txt";
			const string baseName = "ArbitraryFile";
			const string extension = ".txt";

			var f = BaseFolder.File(fileName);
			f.FileName.Should()
				.Be(fileName);
			f.Extension.Should()
				.Be(extension);
			f.FileBaseName.Should()
				.Be(baseName);
		}

		[NotNull,Test]
		public async Task DirectoriesKnowWhetherTheyExist()
		{
			(await BaseFolder.Exists).Should()
				.BeFalse();
			(await BaseFolder.Parent.Exists).Should()
				.BeTrue();
		}

		[NotNull]
		[Test]
		public async Task FilesKnowWhetherTheyExist()
		{
			var testSubject = BaseFolder.File(ArbitraryFileName);
			testSubject.ShouldNotExist();
			await testSubject.Overwrite("anything");
			testSubject.ShouldExist();
		}
	}
}
