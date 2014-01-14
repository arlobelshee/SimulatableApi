// SimulatableAPI
// File: CanDeleteThings.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System.Threading.Tasks;
using NUnit.Framework;
using Simulated.Tests.zzTestHelpers;

namespace Simulated.Tests.FileSystemModification
{
	public abstract class CanDeleteThings : FileSystemTestBase
	{
		[Test]
		public async Task DeletingADirectoryThatDoesNotExistShouldDoNothing()
		{
			var dir = _runRootFolder.Dir(ArbitraryDirName);
			dir.ShouldNotExist();
			await dir.EnsureDoesNotExist();
			dir.ShouldNotExist();
		}

		[Test]
		public async Task DeletingANewlyCreatedDirectoryShouldEliminateItAsIfItWereNeverThere()
		{
			var dir = _runRootFolder.Dir(ArbitraryDirName);
			await dir.EnsureExists();
			dir.ShouldExist();
			await dir.EnsureDoesNotExist();
			dir.ShouldNotExist();
		}

		[Test]
		public async Task RevertingADirectoryDeleteShouldRestoreSubdirsAndFiles()
		{
			var dir = _runRootFolder.Dir(ArbitraryDirName);
			await dir.EnsureExists();
			await dir.Dir(ArbitraryDirName)
				.EnsureExists();
			await dir.File(ArbitraryFileName)
				.Overwrite(ArbitraryContents);
			using (var fs = _testSubject.Clone())
			{
				fs.EnableRevertToHere();
				await fs.Directory(dir.Path)
					.EnsureDoesNotExist();
				dir.ShouldNotExist();
				dir.Dir(ArbitraryDirName)
					.ShouldNotExist();
				dir.File(ArbitraryFileName)
					.ShouldNotExist();
			}
			dir.ShouldExist();
			dir.Dir(ArbitraryDirName)
				.ShouldExist();
			dir.File(ArbitraryFileName)
				.ShouldContain(ArbitraryContents);
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
