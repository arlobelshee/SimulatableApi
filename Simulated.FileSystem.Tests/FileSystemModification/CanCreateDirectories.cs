// SimulatableAPI
// File: CanCreateDirectories.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System.Threading.Tasks;
using NUnit.Framework;
using Simulated.Tests.zzTestHelpers;

namespace Simulated.Tests.FileSystemModification
{
	public abstract class CanCreateDirectories : FileSystemTestBase
	{
		[Test]
		public async Task ShouldBeAbleToCreateADirectory()
		{
			_runRootFolder.ShouldNotExist();
			await _runRootFolder.EnsureExists();
			_runRootFolder.ShouldExist();
		}

		[Test]
		public async Task ShouldBeAbleToRollBackDirectoryCreation()
		{
			await _runRootFolder.EnsureExists();

			_runRootFolder.ShouldExist();
			await _testSubject.RevertAllChanges();
			_runRootFolder.ShouldNotExist();
		}

		[Test]
		public async Task CreatingADirectoryThatAlreadyExistsAndRollingItBackShouldDoNothing()
		{
			await _runRootFolder.EnsureExists();
			_runRootFolder.ShouldExist();
			using (var secondView = _testSubject.Clone())
			{
				secondView.EnableRevertToHere();
				await secondView.Directory(_runRootFolder.Path)
					.EnsureExists();
				_runRootFolder.ShouldExist();
			}
			_runRootFolder.ShouldExist();
		}

		[Test]
		public async Task CreatingADirectoryShouldCreateAnyMissingIntermediateDirectories()
		{
			var subDir = _runRootFolder.Dir("A");

			subDir.Parent.ShouldNotExist();
			await subDir.EnsureExists();
			subDir.Parent.ShouldExist();
		}

		[Test]
		public async Task DirectoriesCreatedBySideEffectOfDeepCreateShouldRollBackCorrectly()
		{
			var subDir = _runRootFolder.Dir("A");
			subDir.Parent.ShouldNotExist();
			await subDir.EnsureExists();

			subDir.Parent.ShouldExist();
			await _testSubject.RevertAllChanges();
			subDir.Parent.ShouldNotExist();
		}

		[Test]
		public async Task OverwritingAFileInAMissingFolderShouldCreateThatFolder()
		{
			_runRootFolder.ShouldNotExist();
			await _runRootFolder.File("CreatedByTest.txt")
				.Overwrite("anything");
			_runRootFolder.ShouldExist();
		}
	}

	[TestFixture]
	public class CanCreateDirectoriesInRealFs : CanCreateDirectories
	{
		protected override FileSystem MakeTestSubject()
		{
			return FileSystem.Real();
		}
	}

	[TestFixture]
	public class CanCreateDirectoriesInMemoryFs : CanCreateDirectories
	{
		protected override FileSystem MakeTestSubject()
		{
			return FileSystem.Simulated();
		}
	}
}
