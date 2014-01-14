// SimulatableAPI
// File: CanNavigateRelativeToAFileOrDirectory.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Simulated.Tests.zzTestHelpers;

namespace Simulated.Tests.FileSystemNavigation
{
	public abstract class CanNavigateRelativeToAFileOrDirectory : FileSystemTestBase
	{
		[Test]
		public void CanGetTheParentOfADirectory()
		{
			_testSubject.Directory(@"C:\Base\Second")
				.Parent.Path.Should()
				.Be(new FsPath(@"C:\Base"));
		}

		[Test]
		public void CanGetAFileWithinADirectory()
		{
			_runRootFolder.File(ArbitraryFileName)
				.FullPath.Should()
				.Be(_runRootFolder.Path/ArbitraryFileName);
		}

		[Test]
		public void CanGetASubDirectory()
		{
			_runRootFolder.Dir(ArbitraryFileName)
				.Path.Should()
				.Be(_runRootFolder.Path/ArbitraryFileName);
		}

		[Test]
		public void AFileKnowsWhereItIs()
		{
			_runRootFolder.File(ArbitraryFileName)
				.ContainingFolder.Should()
				.Be(_runRootFolder);
		}

		[Test]
		public async Task CanGetAllFilesWithinADirectory()
		{
			var firstFile = _runRootFolder.File(ArbitraryFileName);
			var extension = firstFile.Extension;
			var secondFile = _runRootFolder.File("secondFile" + extension);
			await firstFile.Overwrite(ArbitraryContents);
			await secondFile.Overwrite(ArbitraryContents);
			(await _runRootFolder.FilesThatExist("*" + extension))
				.Should()
				.BeEquivalentTo(firstFile, secondFile);
			(await _runRootFolder.FilesThatExist(firstFile.FileBaseName + ".*"))
				.Should()
				.BeEquivalentTo(firstFile);
		}
	}

	[TestFixture]
	public class CanNavigateRelativeToAFileOrDirectoryMemoryFs : CanNavigateRelativeToAFileOrDirectory
	{
		protected override FileSystem MakeTestSubject()
		{
			return FileSystem.Simulated();
		}
	}
}
