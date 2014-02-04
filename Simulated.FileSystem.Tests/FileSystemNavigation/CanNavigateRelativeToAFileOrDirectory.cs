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
			TestSubject.Directory(@"C:\Base\Second")
				.Parent.Path.Should()
				.Be(new FsPath(@"C:\Base"));
		}

		[Test]
		public void CanGetAFileWithinADirectory()
		{
			BaseFolder.File(ArbitraryFileName)
				.FullPath.Should()
				.Be(BaseFolder.Path/ArbitraryFileName);
		}

		[Test]
		public void CanGetASubDirectory()
		{
			BaseFolder.Dir(ArbitraryFileName)
				.Path.Should()
				.Be(BaseFolder.Path/ArbitraryFileName);
		}

		[Test]
		public void AFileKnowsWhereItIs()
		{
			BaseFolder.File(ArbitraryFileName)
				.ContainingFolder.Should()
				.Be(BaseFolder);
		}
	}
}
