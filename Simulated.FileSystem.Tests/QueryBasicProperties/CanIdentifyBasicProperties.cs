// SimulatableAPI
// File: CanIdentifyBasicProperties.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Simulated.Tests.zzTestHelpers;

namespace Simulated.Tests.QueryBasicProperties
{
	public abstract class CanIdentifyBasicProperties : FileSystemTestBase
	{
		[Test]
		public void AFileKnowsItsFileNameParts()
		{
			const string fileName = "ArbitraryFile.txt";
			const string baseName = "ArbitraryFile";
			const string extension = ".txt";

			var f = _runRootFolder.File(fileName);
			f.FileName.Should()
				.Be(fileName);
			f.Extension.Should()
				.Be(extension);
			f.FileBaseName.Should()
				.Be(baseName);
		}

		[Test]
		public void DirectoriesKnowTheirFileSystem()
		{
			_runRootFolder.FileSystem.Should()
				.BeSameAs(_testSubject);
		}

		[Test]
		public void FilesKnowTheirFileSystem()
		{
			_runRootFolder.File("unused.name")
				.FileSystem.Should()
				.BeSameAs(_testSubject);
		}

		[Test]
		public void DirectoriesKnowWhetherTheyExist()
		{
			_runRootFolder.ShouldNotExist();
			_runRootFolder.Parent.ShouldExist();
		}

		[Test]
		public async Task FilesKnowWhetherTheyExist()
		{
			var testSubject = _runRootFolder.File("something.txt");
			testSubject.ShouldNotExist();
			await testSubject.Overwrite("anything");
			testSubject.ShouldExist();
		}
	}

	[TestFixture]
	public class CanIdentifyBasicPropertiesMemoryFs : CanIdentifyBasicProperties
	{
		protected override FileSystem MakeTestSubject()
		{
			return FileSystem.Simulated();
		}
	}
}
