// SimulatableAPI
// File: CanNavigateRelativeToAFileOrDirectory.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using FluentAssertions;
using NUnit.Framework;
using Simulated.Tests.zzTestHelpers;

namespace Simulated.Tests.FileSystemNavigation
{
	public class CanNavigateRelativeToAFileOrDirectory : FileSystemTestBase
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

		protected override FileSystem MakeTestSubject()
		{
			return FileSystem.Simulated();
		}
	}
}
