using System;
using JetBrains.Annotations;
using NUnit.Framework;
using Simulated.Tests.zzTestHelpers;
using Simulated._Fs;

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
			using (FileSystem secondView = _testSubject.Clone())
			{
				secondView.EnableRevertToHere();
				secondView.Directory(_runRootFolder.Path).EnsureExists();
				_runRootFolder.ShouldExist();
			}
			_runRootFolder.ShouldExist();
		}

		[Test]
		public void CreatingADirectoryShouldCreateAnyMissingIntermediateDirectories()
		{
			FsDirectory subDir = _runRootFolder.Dir("A");

			subDir.Parent.ShouldNotExist();
			subDir.EnsureExists();
			subDir.Parent.ShouldExist();
		}

		[Test]
		public void DirectoriesCreatedBySideEffectOfDeepCreateShouldRollBackCorrectly()
		{
			FsDirectory subDir = _runRootFolder.Dir("A");
			subDir.EnsureExists();

			subDir.Parent.ShouldExist();
			_testSubject.RevertAllChanges();
			subDir.Parent.ShouldNotExist();
		}

		[Test]
		public void OverwritingAFileInAMissingFolderShouldCreateThatFolder()
		{
			_runRootFolder.ShouldNotExist();
			_runRootFolder.File("CreatedByTest.txt").Overwrite("anything");
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
