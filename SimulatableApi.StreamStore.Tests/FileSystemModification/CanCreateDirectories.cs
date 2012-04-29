using System;
using System.IO;
using System.Text;
using JetBrains.Annotations;
using NUnit.Framework;
using FluentAssertions;
using SimulatableApi.StreamStore.Tests.zzTestHelpers;

namespace SimulatableApi.StreamStore.Tests.FileSystemModification
{
	public abstract class CanCreateDirectories
	{
		[Test]
		public void CannotGetContentsOfMissingFile()
		{
			FsFile testFile = _testSubject.TempDirectory.File("CreatedByTest.txt");
			_Throws<FileNotFoundException>(() => testFile.ReadAllText(), string.Format("Could not find file '{0}'.", testFile.FullPath.Absolute));
		}

		[Test]
		public void CannotGetContentsOfFolder()
		{
			FsFile testFile = _runRootFolder.File("CreatedByTest.txt");
			_testSubject.Directory(testFile.FullPath).Create();
			_Throws<UnauthorizedAccessException>(() => testFile.ReadAllText(), string.Format("Access to the path '{0}' is denied.", testFile.FullPath.Absolute));
		}

		[Test]
		public void StringsShouldBeEncodedInUtf8ByDefault()
		{
			FsFile testFile = _runRootFolder.File("CreatedByTest.txt");
			testFile.Overwrite(NewContents);
			var asString = testFile.ReadAllBytes();
			asString.Should().Equal(Encoding.UTF8.GetBytes(NewContents));
		}

		[Test]
		public void BinaryFilesWithValidStringsCanBeReadAsText()
		{
			FsFile testFile = _runRootFolder.File("CreatedByTest.txt");
			testFile.OverwriteBinary(Encoding.UTF8.GetBytes(NewContents));
			var asString = testFile.ReadAllText();
			asString.Should().Be(NewContents);
		}

		[Test]
		public void ShouldBeAbleToRollBackDirectoryCreation()
		{
			_runRootFolder.ShouldNotExist();
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
			FsDirectory theFolder = _runRootFolder.Dir("A");

			theFolder.Parent.ShouldNotExist();
			theFolder.ShouldNotExist();

			theFolder.Create();

			theFolder.ShouldExist();
			theFolder.Parent.ShouldExist();
		}

		[Test]
		public void DirectoriesCreatedBySideEffectOfDeepCreateShouldRollBackCorrectly()
		{
			FsDirectory theFolder = _runRootFolder.Dir("A");

			theFolder.Parent.ShouldNotExist();

			theFolder.Create();
			theFolder.Parent.ShouldExist();

			_testSubject.RevertAllChanges();
			theFolder.Parent.ShouldNotExist();
		}

		[Test]
		public void OverwritingAFileInAMissingFolderShouldCreateThatFolder()
		{
			FsDirectory parentFolder = _runRootFolder;
			FsFile file = parentFolder.File("CreatedByTest.txt");
			file.Overwrite(NewContents);
			Assert.That(parentFolder.Exists);
		}

		[Test]
		public void CanCreateDirectoryAndRevertIt()
		{
			FsDirectory newDir = _runRootFolder;
			newDir.ShouldNotExist();
			newDir.Create();
			newDir.ShouldExist();
			_testSubject.RevertAllChanges();
			newDir.ShouldNotExist();
		}

		[Test]
		public void CanCreateFileAndRevertIt()
		{
			FsFile newFile = _runRootFolder.File("CreatedByTest.txt");
			newFile.ShouldNotExist();
			newFile.Overwrite(OriginalContents);
			newFile.ShouldContain(OriginalContents);
			_testSubject.RevertAllChanges();
			newFile.ShouldNotExist();
		}

		[Test]
		public void CanOverwriteFileAndRevertIt()
		{
			FsFile newFile = _runRootFolder.File("CreatedByTest.txt");
			newFile.Overwrite(OriginalContents);
			using (FileSystem secondView = _testSubject.Clone())
			{
				secondView.EnableRevertToHere();
				newFile.ShouldContain(OriginalContents);
				secondView.Directory(_runRootFolder.Path).File("CreatedByTest.txt").Overwrite(NewContents);
				newFile.ShouldContain(NewContents);
			}
			newFile.ShouldContain(OriginalContents);
		}

		private static void _Throws<TException>(TestDelegate code, string message) where TException : Exception
		{
			Assert.That(Assert.Throws<TException>(code), Has.Property("Message").EqualTo(message));
		}

		private const string OriginalContents = "Original contents";
		private const string NewContents = "helȽo ﺷ";
		[NotNull]
		private FileSystem _testSubject;
		[NotNull]
		private FsDirectory _runRootFolder;

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
