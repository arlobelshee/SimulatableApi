// SimulatableAPI
// File: CanDeleteThings.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using NUnit.Framework;
using Simulated.Tests.zzTestHelpers;

namespace Simulated.Tests.FileSystemModification
{
	public abstract class CanDeleteThings : FileSystemTestBase
	{
		[Test]
		public void DeletingADirectoryThatDoesNotExistShouldDoNothing()
		{
			var dir = _runRootFolder.Dir(ArbitraryDirName);
			dir.ShouldNotExist();
			dir.EnsureDoesNotExist();
			dir.ShouldNotExist();
		}

		[Test]
		public void DeletingANewlyCreatedDirectoryShouldEliminateItAsIfItWereNeverThere()
		{
			var dir = _runRootFolder.Dir(ArbitraryDirName);
			dir.EnsureExists();
			dir.ShouldExist();
			dir.EnsureDoesNotExist();
			dir.ShouldNotExist();
		}
	}

	[TestFixture]
	public class CanDeleteThingsInRealFs : CanDeleteThings
	{
		protected override FileSystem MakeTestSubject()
		{
			return FileSystem.Real();
		}
	}

	[TestFixture]
	public class CanDeleteThingsInMemoryFs : CanDeleteThings
	{
		protected override FileSystem MakeTestSubject()
		{
			return FileSystem.Simulated();
		}
	}
}
