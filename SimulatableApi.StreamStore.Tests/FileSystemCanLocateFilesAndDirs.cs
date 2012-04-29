using System;
using System.IO;
using System.Text;
using JetBrains.Annotations;
using NUnit.Framework;
using FluentAssertions;

namespace SimulatableApi.StreamStore.Tests
{
	public class RealFileSystemCanLocateFilesAndDirs
	{
		private const string ArbitraryMissingFolder = @"C:\theroot\folder";
		private const string OriginalContents = "Original contents";
		private const string NewContents = "helȽo ﺷ";
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
		protected virtual FileSystem MakeTestSubject()
		{
			return FileSystem.Real();
		}

		[Test]
		public void CanCheckForExistence()
		{
			Assert.That(_testSubject.Directory(ArbitraryMissingFolder), Has.Property("Exists").False);
		}

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
		public void CreatingADirectoryThatAlreadyExistsDoesNothingAndRollbackDoesNothing()
		{
			_runRootFolder.Create();
			Assert.That(_runRootFolder.Exists);
			using (FileSystem secondView = _testSubject.Clone())
			{
				secondView.EnableRevertToHere();
				secondView.Directory(_runRootFolder.Path).Create();
				Assert.That(_runRootFolder.Exists);
			}
			Assert.That(_runRootFolder.Exists);
		}

		[Test]
		public void CreatingAPathWithMultipleDirsThenRollingBackRemovesAllCreatedDirs()
		{
			FsDirectory theFolder = _runRootFolder.Dir("A");
			FsDirectory parentFolderCreatedInPassing = theFolder.Parent;
			FsDirectory root = parentFolderCreatedInPassing.Parent;

			Assert.That(!parentFolderCreatedInPassing.Exists);
			Assert.That(root.Exists);

			theFolder.Create();
			Assert.That(theFolder.Exists);
			Assert.That(parentFolderCreatedInPassing.Exists);
			Assert.That(root.Exists);

			_testSubject.RevertAllChanges();
			Assert.That(!theFolder.Exists);
			Assert.That(!parentFolderCreatedInPassing.Exists);
			Assert.That(root.Exists);
		}

		[Test]
		public void OverwritingAFileInAMissingFolderCreatesThatFolder()
		{
			FsDirectory parentFolder = _runRootFolder;
			FsFile file = parentFolder.File("CreatedByTest.txt");
			file.Overwrite(NewContents);
			Assert.That(parentFolder.Exists);
		}

		[Test]
		public void CanGetTheParentOfADirectory()
		{
			Assert.That(_testSubject.Directory(@"C:\Base\Second").Parent, Is.EqualTo(_testSubject.Directory(@"C:\Base")));
		}

		[Test]
		public void CanCreateDirectoryAndRevertIt()
		{
			FsDirectory newDir = _runRootFolder;
			_AssertIsMissing(newDir);
			newDir.Create();
			_AssertIsDir(newDir);
			_testSubject.RevertAllChanges();
			_AssertIsMissing(newDir);
		}

		[Test]
		public void CanCreateFileAndRevertIt()
		{
			FsFile newFile = _runRootFolder.File("CreatedByTest.txt");
			_AssertIsMissing(newFile);
			newFile.Overwrite(OriginalContents);
			_AssertIsFile(newFile, OriginalContents);
			_testSubject.RevertAllChanges();
			_AssertIsMissing(newFile);
		}

		[Test]
		public void CanOverwriteFileAndRevertIt()
		{
			FsFile newFile = _runRootFolder.File("CreatedByTest.txt");
			newFile.Overwrite(OriginalContents);
			using (FileSystem secondView = _testSubject.Clone())
			{
				secondView.EnableRevertToHere();
				_AssertIsFile(newFile, OriginalContents);
				secondView.Directory(_runRootFolder.Path).File("CreatedByTest.txt").Overwrite(NewContents);
				_AssertIsFile(newFile, NewContents);
			}
			_AssertIsFile(newFile, OriginalContents);
		}

		[Test]
		public void AFileKnowsWhereItIs()
		{
			const string fileName = "ArbitraryFile.txt";
			const string extension = ".txt";
			FsFile f = _runRootFolder.File(fileName);
			Assert.That(f.ContainingFolder, Is.EqualTo(_runRootFolder));
			Assert.That(f.FileName, Is.EqualTo(fileName));
			Assert.That(f.Extension, Is.EqualTo(extension));
		}

		[Test]
		public void FileSystemCanMakeAFileFromAString()
		{
			const string fileName = "SomeFileName.html";
			Assert.That(_testSubject.File((_runRootFolder.Path/fileName).Absolute), Is.EqualTo(_runRootFolder.File(fileName)));
		}

		private static void _AssertIsMissing(object node)
		{
			Assert.That(node, Has.Property("Exists").False);
		}

		private static void _AssertIsDir(FsDirectory dir)
		{
			Assert.That(dir, Has.Property("Exists").True);
		}

		private static void _AssertIsFile([NotNull] FsFile file, string contents)
		{
			Assert.That(file, Has.Property("Exists").True);
			Assert.That(file.ReadAllText(), Is.EqualTo(contents));
		}

		private static void _Throws<TException>(TestDelegate code, string message) where TException : Exception
		{
			Assert.That(Assert.Throws<TException>(code), Has.Property("Message").EqualTo(message));
		}
	}

	[TestFixture]
	public class SimulatedFileSystemCanLocateFilesAndDirs : RealFileSystemCanLocateFilesAndDirs
	{
		protected override FileSystem MakeTestSubject()
		{
			return FileSystem.Simulated();
		}
	}
}
