using System;
using System.IO;
using JetBrains.Annotations;
using NUnit.Framework;

namespace SimulatableApi.Tests
{
	public class RealFileSystemCanLocateFilesAndDirs
	{
		private const string ArbitraryMissingFolder = @"C:\theroot\folder";
		private const string OriginalContents = "Original contents";
		private const string NewContents = "New contents";
		[NotNull] private FileSystem _testSubject;

		[SetUp]
		public void Setup()
		{
			_testSubject = MakeTestSubject();
			_testSubject.EnableRevertToHere();
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
		public void CanMakeDirectoryReference()
		{
			Assert.That(_testSubject.Directory(ArbitraryMissingFolder).Path.Absolute, Is.EqualTo(ArbitraryMissingFolder));
		}

		[Test]
		public void RejectsInvalidDirectories()
		{
			_Throws<ArgumentNullException>(() => _testSubject.Directory(string.Empty), "A path cannot be null or empty.\r\nParameter name: absolutePath");
		}

		[Test]
		public void RejectsInvalidFiles()
		{
			_Throws<ArgumentNullException>(() => _testSubject.File(string.Empty), "A path cannot be null or empty.\r\nParameter name: absolutePath");
		}

		[Test]
		public void CannotAskForTheParentOfRoot()
		{
			_Throws<InvalidOperationException>(() => { var foo = _testSubject.Directory(@"C:\").Parent; }, "The root directory does not have a parent.");
		}

		[Test]
		public void CannotGetContentsOfMissingFile()
		{
			var testFile = _TestFile(_testSubject);
			_Throws<FileNotFoundException>(() => { var foo = testFile.ReadAllText(); }, string.Format("Could not find file '{0}'.", testFile.FullPath.Absolute));
		}

		[Test]
		public void CannotGetContentsOfFolder()
		{
			var testFile = _TestFile(_testSubject);
			_testSubject.Directory(testFile.FullPath).Create();
			_Throws<UnauthorizedAccessException>(() => { var foo = testFile.ReadAllText(); },
				string.Format("Access to the path '{0}' is denied.", testFile.FullPath.Absolute));
		}

		[Test]
		public void CreatingADirectoryThatAlreadyExistsDoesNothingAndRollbackDoesNothing()
		{
			var theFolder = _testSubject.Directory(ArbitraryMissingFolder);
			theFolder.Create();
			Assert.That(theFolder.Exists);
			using (var secondView = _testSubject.Clone())
			{
				secondView.EnableRevertToHere();
				secondView.Directory(theFolder.Path).Create();
				Assert.That(theFolder.Exists);
			}
			Assert.That(theFolder.Exists);
		}

		[Test]
		public void CreatingAPathWithMultipleDirsThenRollingBackRemovesAllCreatedDirs()
		{
			var theFolder = _testSubject.Directory(ArbitraryMissingFolder);
			var parentFolderCreatedInPassing = theFolder.Parent;
			var root = parentFolderCreatedInPassing.Parent;

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
			var parentFolder = _testSubject.Directory(ArbitraryMissingFolder);
			var file = parentFolder.File("CreatedByTest.txt");
			file.Overwrite(NewContents);
			Assert.That(parentFolder.Exists);
		}

		[Test]
		public void DirectoriesAreTheSameWhetherCreatedWithTrailingSlashOrNot()
		{
			Assert.That(_testSubject.Directory(@"C:\Path\").Path.Absolute, Is.EqualTo(@"C:\Path"));
		}

		[Test]
		public void CanGetTheParentOfADirectory()
		{
			Assert.That(_testSubject.Directory(@"C:\Base\Second").Parent, Is.EqualTo(_testSubject.Directory(@"C:\Base\")));
		}

		[Test]
		public void CanLocateTempFolder()
		{
			var tempPath = Path.GetTempPath();
			tempPath = tempPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			Assert.That(_testSubject.TempDirectory.Path.Absolute, Is.EqualTo(tempPath));
		}

		[Test]
		public void CanCreateDirectoryAndRevertIt()
		{
			var newDir = _testSubject.TempDirectory.Dir("CreatedByTest");
			_AssertIsMissing(newDir);
			newDir.Create();
			_AssertIsDir(newDir);
			_testSubject.RevertAllChanges();
			_AssertIsMissing(newDir);
		}

		[Test]
		public void CanCreateFileAndRevertIt()
		{
			var newFile = _TestFile(_testSubject);
			_AssertIsMissing(newFile);
			newFile.Overwrite(OriginalContents);
			_AssertIsFile(newFile, OriginalContents);
			_testSubject.RevertAllChanges();
			_AssertIsMissing(newFile);
		}

		[Test]
		public void CanOverwriteFileAndRevertIt()
		{
			var newFile = _TestFile(_testSubject);
			newFile.Overwrite(OriginalContents);
			using (var secondView = _testSubject.Clone())
			{
				secondView.EnableRevertToHere();
				_AssertIsFile(newFile, OriginalContents);
				_TestFile(secondView).Overwrite(NewContents);
				_AssertIsFile(newFile, NewContents);
			}
			_AssertIsFile(newFile, OriginalContents);
		}

		[Test]
		public void AFileKnowsWhereItIs()
		{
			const string fileName = "ArbitraryFile.txt";
			const string extension = ".txt";
			var dir = _testSubject.TempDirectory;
			var f = dir.File(fileName);
			Assert.That(f.ContainingFolder, Is.EqualTo(dir));
			Assert.That(f.FileName, Is.EqualTo(fileName));
			Assert.That(f.Extension, Is.EqualTo(extension));
		}

		[Test]
		public void FileSystemCanMakeAFileFromAString()
		{
			const string fileName = "SomeFileName.html";
			var dir = _testSubject.TempDirectory;
			Assert.That(_testSubject.File((dir.Path/fileName).Absolute), Is.EqualTo(dir.File(fileName)));
		}

		private static FsFile _TestFile([NotNull] FileSystem fs)
		{
			return fs.TempDirectory.File("CreatedByTest.txt");
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
