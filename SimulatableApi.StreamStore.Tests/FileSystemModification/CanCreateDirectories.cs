using System;
using JetBrains.Annotations;
using NUnit.Framework;
using Simulated.Tests.zzTestHelpers;

namespace Simulated.Tests.FileSystemModification
{
	public abstract class CanCreateDirectories
	{
		[Test]
		public void ShouldBeAbleToCreateADirectory()
		{
			_runRootFolder.ShouldNotExist();
			_runRootFolder.Create();
			_runRootFolder.ShouldExist();
		}

		[Test]
		public void ShouldBeAbleToRollBackDirectoryCreation()
		{
			_runRootFolder.Create();

			_runRootFolder.ShouldExist();
			_testSubject.RevertAllChanges();
			_runRootFolder.ShouldNotExist();
		}

		[Test]
		public void CreatingADirectoryThatAlreadyExistsAndRollingItBackShouldDoNothing()
		{
			_runRootFolder.Create();
			_runRootFolder.ShouldExist();
			using (FileSystem secondView = _testSubject.Clone())
			{
				secondView.EnableRevertToHere();
				secondView.Directory(_runRootFolder.Path).Create();
				_runRootFolder.ShouldExist();
			}
			_runRootFolder.ShouldExist();
		}

		[Test]
		public void CreatingADirectoryShouldCreateAnyMissingIntermediateDirectories()
		{
			FsDirectory subDir = _runRootFolder.Dir("A");

			subDir.Parent.ShouldNotExist();
			subDir.Create();
			subDir.Parent.ShouldExist();
		}

		[Test]
		public void DirectoriesCreatedBySideEffectOfDeepCreateShouldRollBackCorrectly()
		{
			FsDirectory subDir = _runRootFolder.Dir("A");
			subDir.Create();

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

		[NotNull] private FileSystem _testSubject;
		[NotNull] private FsDirectory _runRootFolder;

		[SetUp]
		public void Setup()
		{
			_testSubject = MakeTestSubject();
			_testSubject.EnableRevertToHere();
			_runRootFolder = _testSubject.TempDirectory.Dir("CreatedByTestRun-" + Guid.NewGuid());
		}

		[TearDown]
		public void Teardown()
		{
			_testSubject.RevertAllChanges();
		}

		[NotNull]
		protected abstract FileSystem MakeTestSubject();
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
