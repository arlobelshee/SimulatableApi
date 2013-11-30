// SimulatableAPI
// File: CanCreateDirectories.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using NUnit.Framework;
using Simulated.Tests.zzTestHelpers;

namespace Simulated.Tests.FileSystemModification
{
	public abstract class CanCreateDirectories : FileSystemTestBase
	{
		[Test]
		public void ShouldBeAbleToCreateADirectory()
		{
			_runRootFolder.ShouldNotExist();
			_runRootFolder.EnsureExists();
			_runRootFolder.ShouldExist();
		}

		[Test]
		public void ShouldBeAbleToRollBackDirectoryCreation()
		{
			_runRootFolder.EnsureExists();

			_runRootFolder.ShouldExist();
			_testSubject.RevertAllChanges();
			_runRootFolder.ShouldNotExist();
		}

		[Test]
		public void CreatingADirectoryThatAlreadyExistsAndRollingItBackShouldDoNothing()
		{
			_runRootFolder.EnsureExists();
			_runRootFolder.ShouldExist();
			using (var secondView = _testSubject.Clone())
			{
				secondView.EnableRevertToHere();
				secondView.Directory(_runRootFolder.Path)
					.EnsureExists();
				_runRootFolder.ShouldExist();
			}
			_runRootFolder.ShouldExist();
		}

		[Test]
		public void CreatingADirectoryShouldCreateAnyMissingIntermediateDirectories()
		{
			var subDir = _runRootFolder.Dir("A");

			subDir.Parent.ShouldNotExist();
			subDir.EnsureExists();
			subDir.Parent.ShouldExist();
		}

		[Test]
		public void DirectoriesCreatedBySideEffectOfDeepCreateShouldRollBackCorrectly()
		{
			var subDir = _runRootFolder.Dir("A");
			subDir.EnsureExists();

			subDir.Parent.ShouldExist();
			_testSubject.RevertAllChanges();
			subDir.Parent.ShouldNotExist();
		}

		[Test]
		public void OverwritingAFileInAMissingFolderShouldCreateThatFolder()
		{
			_runRootFolder.ShouldNotExist();
			_runRootFolder.File("CreatedByTest.txt")
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
