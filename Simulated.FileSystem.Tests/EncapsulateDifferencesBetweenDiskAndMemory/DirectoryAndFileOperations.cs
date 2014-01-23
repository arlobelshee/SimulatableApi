// SimulatableAPI
// File: DirectoryAndFileOperations.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System;
using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using JetBrains.Annotations;
using NUnit.Framework;
using Simulated.Tests.zzTestHelpers;
using Simulated._Fs;

namespace Simulated.Tests.EncapsulateDifferencesBetweenDiskAndMemory
{
	[TestFixture]
	public abstract class DirectoryAndFileOperations
	{
		[SetUp]
		public void Setup()
		{
			_testSubject = _MakeTestSubject();
			var runName = "TestRun-" + Guid.NewGuid()
				.ToString("N");
			_baseFolder = FsPath.TempFolder/runName;
			_testSubject.CreateDir(_baseFolder);
		}

		[TearDown]
		public void Teardown()
		{
			_testSubject.DeleteDir(_baseFolder);
		}

		[Test]
		public void CanCreateFileAndReadItsContents()
		{
			var fileName = _baseFolder/"file.txt";
			_testSubject.ShouldNotExist(fileName);
			_testSubject.Overwrite(fileName, ArbitraryFileContents);
			_testSubject.DirExists(fileName)
				.Should()
				.BeFalse();
			_testSubject.FileExists(fileName)
				.Should()
				.BeTrue();
			_testSubject.TextContents(fileName)
				.Should()
				.Be(ArbitraryFileContents);
		}

		[Test]
		public void CanCreateBinaryFileAndReadItsContents()
		{
			var fileName = _baseFolder/"file.txt";
			_testSubject.ShouldNotExist(fileName);
			var contents = Encoding.UTF8.GetBytes(ArbitraryFileContents);
			_testSubject.Overwrite(fileName, contents);
			_testSubject.FileExists(fileName)
				.Should()
				.BeTrue();
			_testSubject.RawContents(fileName)
				.Should()
				.Equal(contents);
		}

		[Test]
		public void WritingToFileInMissingDirectoryShouldCreateParentDirs()
		{
			var fileName = _baseFolder/"parent"/"file.txt";
			_testSubject.Overwrite(fileName, ArbitraryFileContents);
			_testSubject.ShouldBeDir(_baseFolder/"parent");
		}

		[Test]
		public void WritingBinaryContentsToFileInMissingDirectoryShouldCreateParentDirs()
		{
			var fileName = _baseFolder/"parent"/"file.txt";
			_testSubject.Overwrite(fileName, Encoding.UTF8.GetBytes(ArbitraryFileContents));
			_testSubject.ShouldBeDir(_baseFolder/"parent");
		}

		[Test]
		public void NewDirectoryShouldExistWhenCreated()
		{
			var newPath = _baseFolder/"sub";
			_testSubject.DirExists(newPath)
				.Should()
				.BeFalse();
			_testSubject.CreateDir(newPath);
			_testSubject.DirExists(newPath)
				.Should()
				.BeTrue();
			_testSubject.FileExists(newPath)
				.Should()
				.BeFalse();
		}

		[Test]
		public void CreatingNewDirectoryShouldCreateAllParents()
		{
			_testSubject.CreateDir(_baseFolder/"one"/"two"/"three");
			_testSubject.ShouldBeDir(_baseFolder/"one");
			_testSubject.ShouldBeDir(_baseFolder/"one"/"two");
			_testSubject.ShouldBeDir(_baseFolder/"one"/"two"/"three");
		}

		[Test]
		public void CreatingDirectoryWhichExistsShouldNoop()
		{
			var newPath = _baseFolder/"sub";
			var filePath = newPath/"file.txt";
			_testSubject.CreateDir(newPath);
			_testSubject.Overwrite(filePath, ArbitraryFileContents);
			_testSubject.CreateDir(newPath);
			_testSubject.DirExists(newPath)
				.Should()
				.BeTrue();
			_testSubject.ShouldBeFile(filePath, ArbitraryFileContents);
		}

		[Test]
		public void MovingDirectoryShouldMoveAllContents()
		{
			var originalRoot = _baseFolder/"original";
			var newRoot = _baseFolder/"new";
			var filePath = originalRoot/"something"/"file.txt";
			_testSubject.Overwrite(filePath, ArbitraryFileContents);
			_testSubject.MoveDir(originalRoot, newRoot);
			_testSubject.ShouldBeDir(newRoot);
			_testSubject.ShouldNotExist(originalRoot);
			_testSubject.ShouldBeFile(newRoot/"something"/"file.txt", ArbitraryFileContents);
		}

		[Test]
		public void CreatingFileShouldCreateMissingIntermediateDirectories()
		{
			var dirName = _baseFolder/"missing_dir";
			var fileName = dirName/"file.txt";
			_testSubject.Overwrite(fileName, ArbitraryFileContents);
			_testSubject.ShouldBeFile(fileName, ArbitraryFileContents);
			_testSubject.ShouldBeDir(dirName);
		}

		[Test]
		[TestCase("matches.*", "matches.txt", "matches.jpg")]
		[TestCase("*.*", "matches.txt", "matches.jpg", "no_match.txt")]
		[TestCase("*.txt", "matches.txt", "no_match.txt")]
		[TestCase("matches.txt", "matches.txt")]
		public void FileMatchingShouldMatchStarPatterns([NotNull] string searchPattern, [NotNull] params string[] expectedMatches)
		{
			_testSubject.Overwrite(_baseFolder/"matches.txt", ArbitraryFileContents);
			_testSubject.Overwrite(_baseFolder/"matches.jpg", ArbitraryFileContents);
			_testSubject.Overwrite(_baseFolder/"no_match.txt", ArbitraryFileContents);
			_testSubject.FindFiles(_baseFolder, searchPattern)
				.Should()
				.BeEquivalentTo(expectedMatches.Select(m => _baseFolder/m));
		}

		[Test]
		public void CannotReadContentsOfMissingFile()
		{
			var missingFileName = _baseFolder/"missing.txt";
			Action readMissingFile = () => _testSubject.TextContents(missingFileName);
			readMissingFile.ShouldThrow<FileNotFoundException>()
				.WithMessage(string.Format("Could not find file '{0}'.", missingFileName));
		}

		[Test]
		public void CannotReadContentsOfFolder()
		{
			var dirName = _baseFolder/"directory.git";
			_testSubject.CreateDir(dirName);
			Action readMissingFile = () => _testSubject.TextContents(dirName);
			readMissingFile.ShouldThrow<UnauthorizedAccessException>()
				.WithMessage(string.Format("Access to the path '{0}' is denied.", dirName));
		}

		[Test]
		public void DeletedDirectoryShouldNotExist()
		{
			var newPath = _baseFolder/"sub";
			_testSubject.CreateDir(newPath);
			_testSubject.ShouldBeDir(newPath);
			_testSubject.DeleteDir(newPath);
			_testSubject.ShouldNotExist(newPath);
		}

		[Test]
		public void DeletedFileShouldNotExist()
		{
			var newPath = _baseFolder/"sub.txt";
			_testSubject.Overwrite(newPath, ArbitraryFileContents);
			_testSubject.DeleteFile(newPath);
			_testSubject.ShouldNotExist(newPath);
		}

		[Test]
		public void UsingMoveDirectoryOnAFileShouldDenyAccess()
		{
			var fileName = _baseFolder/"sub.txt";
			_testSubject.Overwrite(fileName, ArbitraryFileContents);
			Action moveDir = () => _testSubject.MoveDir(fileName, _baseFolder/"dir");
			moveDir.ShouldThrow<UnauthorizedAccessException>()
				.WithMessage(string.Format("Cannot move the directory '{0}' because it is a file.", fileName));
		}

		[Test]
		public void UsingMoveFileOnADirectoryShouldFailToFindFile()
		{
			var dirName = _baseFolder/"sub";
			_testSubject.CreateDir(dirName);
			Action moveFile = () => _testSubject.MoveFile(dirName, _baseFolder/"dest.txt");
			moveFile.ShouldThrow<FileNotFoundException>()
				.WithMessage(string.Format("Could not find file '{0}'.", dirName));
		}

		[Test]
		public void MovingMissingFileShouldFailToFindFile()
		{
			var dirName = _baseFolder/"src.txt";
			Action moveFile = () => _testSubject.MoveFile(dirName, _baseFolder/"dest.txt");
			moveFile.ShouldThrow<FileNotFoundException>()
				.WithMessage(string.Format("Could not find file '{0}'.", dirName));
		}

		[Test]
		public void MovingMissingDirectoryShouldFailToFindDirectory()
		{
			Action moveDir = () => _testSubject.MoveDir(_baseFolder/"src", _baseFolder / "dest");
			moveDir.ShouldThrow<DirectoryNotFoundException>()
				.WithMessage(string.Format("Could not find a part of the path '{0}'.", _baseFolder/"src"));
		}

		[Test]
		public void DeletingMissingDirectoryShouldNoop()
		{
			var newPath = _baseFolder/"sub";
			_testSubject.ShouldNotExist(newPath);
			_testSubject.DeleteDir(newPath);
			_testSubject.ShouldNotExist(newPath);
		}

		[Test]
		public void DeletingMissingFileShouldNoop()
		{
			var newPath = _baseFolder/"sub.txt";
			_testSubject.ShouldNotExist(newPath);
			_testSubject.DeleteFile(newPath);
			_testSubject.ShouldNotExist(newPath);
		}

		[Test]
		public void UsingDeleteDirectoryOnAFileShouldNoOp()
		{
			var newPath = _baseFolder/"sub.txt";
			_testSubject.Overwrite(newPath, ArbitraryFileContents);
			_testSubject.DeleteDir(newPath);
			_testSubject.ShouldBeFile(newPath, ArbitraryFileContents);
		}

		[Test]
		public void UsingDeleteFileOnADirectoryShouldRefuseAccess()
		{
			var dirName = _baseFolder / "sub";
			_testSubject.CreateDir(dirName);
			Action deleteFile = () => _testSubject.DeleteFile(dirName);
			deleteFile.ShouldThrow<UnauthorizedAccessException>()
				.WithMessage(string.Format("Access to the path '{0}' is denied.", dirName));
		}

		[Test]
		public void MovingADirectoryToAnExistingFileShouldFail()
		{
			_testSubject.CreateDir(_baseFolder/"src");
			_testSubject.Overwrite(_baseFolder/"dest", ArbitraryFileContents);
			Action move = ()=>_testSubject.MoveDir(_baseFolder/"src", _baseFolder/"dest");
			move.ShouldThrow<IOException>()
				.WithMessage("Cannot create a file when that file already exists.\r\n");
		}

		[Test]
		public void MovingADirectoryToAnExistingDirectoryShouldFail()
		{
			_testSubject.CreateDir(_baseFolder/"src");
			_testSubject.CreateDir(_baseFolder/"dest");
			Action move = ()=>_testSubject.MoveDir(_baseFolder/"src", _baseFolder/"dest");
			move.ShouldThrow<IOException>()
				.WithMessage("Cannot create a file when that file already exists.\r\n");
		}

		[Test]
		public void MovingAFileToAnExistingFileShouldFail()
		{
			_testSubject.Overwrite(_baseFolder/"src", ArbitraryFileContents);
			_testSubject.Overwrite(_baseFolder/"dest", ArbitraryFileContents);
			Action move = ()=>_testSubject.MoveFile(_baseFolder/"src", _baseFolder/"dest");
			move.ShouldThrow<IOException>()
				.WithMessage("Cannot create a file when that file already exists.\r\n");
		}

		[Test]
		public void MovingAFileToAnExistingDirectoryShouldFail()
		{
			_testSubject.Overwrite(_baseFolder/"src", ArbitraryFileContents);
			_testSubject.CreateDir(_baseFolder/"dest");
			Action move = ()=>_testSubject.MoveFile(_baseFolder/"src", _baseFolder/"dest");
			move.ShouldThrow<IOException>()
				.WithMessage("Cannot create a file when that file already exists.\r\n");
		}

		[Test]
		public void StringsShouldBeEncodedInUtf8ByDefault()
		{
			var testFile = _baseFolder/"hello.txt";
			_testSubject.Overwrite(testFile, UnicodeContents);
			var asBytes = _testSubject.RawContents(testFile);
			asBytes.Should()
				.Equal(Encoding.UTF8.GetBytes(UnicodeContents));
		}

		[Test]
		public void BinaryFilesWithValidStringDataShouldBeReadableAsText()
		{
			var testFile = _baseFolder/"hello.txt";
			_testSubject.Overwrite(testFile, Encoding.UTF8.GetBytes(UnicodeContents));
			var asString = _testSubject.TextContents(testFile);
			asString.Should()
				.Be(UnicodeContents);
		}

		private const string ArbitraryFileContents = "contents";
		private const string UnicodeContents = "helȽo ﺷ";

		[NotNull]
		internal abstract _IFsDisk _MakeTestSubject();

		private FsPath _baseFolder;
		private _IFsDisk _testSubject;
	}

	public class DirectoryAndFileOperationsDiskFs : DirectoryAndFileOperations
	{
		internal override _IFsDisk _MakeTestSubject()
		{
			return new _DiskReal();
		}
	}

	public class DirectoryAndFileOperationsMemoryFs : DirectoryAndFileOperations
	{
		internal override _IFsDisk _MakeTestSubject()
		{
			return new _DiskSimulated();
		}
	}
}
