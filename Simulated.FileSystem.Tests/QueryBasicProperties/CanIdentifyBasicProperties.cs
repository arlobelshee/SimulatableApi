﻿// SimulatableAPI
// File: CanIdentifyBasicProperties.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using FluentAssertions;
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

		[Test]
		public void DirectoriesKnowWhetherTheyExist()
		{
			BaseFolder.Exists.Should()
				.BeFalse();
			BaseFolder.Parent.Exists.Should()
				.BeTrue();
		}

		[Test]
		public void FilesKnowWhetherTheyExist()
		{
			var testSubject = BaseFolder.File(ArbitraryFileName);
			testSubject.Exists.Should()
				.BeFalse();
			testSubject.Overwrite("anything");
			testSubject.Exists.Should()
				.BeTrue();
		}
	}
}
