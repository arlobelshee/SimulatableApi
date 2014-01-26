// SimulatableAPI
// File: DirectoryAndFileOperations - Copy.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System;
using System.IO;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using Simulated.Tests.zzTestHelpers;
using Simulated._Fs;

namespace Simulated.Tests.EncapsulateDifferencesBetweenDiskAndMemory
{
	[TestFixture]
	public abstract class CanReadAndWriteFileContentsAtomically : DiskTestBase
	{
		[Test]
		public void CanCreateFileAndReadItsContents()
		{
			var fileName = BaseFolder/"file.txt";
			TestSubject.ShouldNotExist(fileName);
			TestSubject.Overwrite(fileName, ArbitraryFileContents);
			TestSubject.DirExists(fileName)
				.Should()
				.BeFalse();
			TestSubject.FileExists(fileName)
				.Should()
				.BeTrue();
			TestSubject.TextContents(fileName)
				.Should()
				.Be(ArbitraryFileContents);
		}

		[Test]
		public void CanCreateBinaryFileAndReadItsContents()
		{
			var fileName = BaseFolder/"file.txt";
			TestSubject.ShouldNotExist(fileName);
			var contents = Encoding.UTF8.GetBytes(ArbitraryFileContents);
			TestSubject.Overwrite(fileName, contents);
			TestSubject.FileExists(fileName)
				.Should()
				.BeTrue();
			TestSubject.RawContents(fileName)
				.Should()
				.Equal(contents);
		}

		[Test]
		public void WritingToFileInMissingDirectoryShouldCreateParentDirs()
		{
			var fileName = BaseFolder/"parent"/"file.txt";
			TestSubject.Overwrite(fileName, ArbitraryFileContents);
			TestSubject.ShouldBeDir(BaseFolder/"parent");
		}

		[Test]
		public void WritingBinaryContentsToFileInMissingDirectoryShouldCreateParentDirs()
		{
			var fileName = BaseFolder/"parent"/"file.txt";
			TestSubject.Overwrite(fileName, Encoding.UTF8.GetBytes(ArbitraryFileContents));
			TestSubject.ShouldBeDir(BaseFolder/"parent");
		}

		[Test]
		public void CannotReadContentsOfMissingFile()
		{
			var missingFileName = BaseFolder/"missing.txt";
			Action readMissingFile = () => TestSubject.TextContents(missingFileName);
			readMissingFile.ShouldThrow<BadStorageRequest>()
				.WithMessage(string.Format(UserMessages.ReadErrorFileNotFound, missingFileName));
		}

		[Test]
		public void CannotReadContentsOfMissingFileAsRawBinary()
		{
			var missingFileName = BaseFolder/"missing.txt";
			Action readMissingFile = () => TestSubject.RawContents(missingFileName);
			readMissingFile.ShouldThrow<BadStorageRequest>()
				.WithMessage(string.Format(UserMessages.ReadErrorFileNotFound, missingFileName));
		}

		[Test]
		public void CannotReadContentsOfFolder()
		{
			var dirName = BaseFolder/"directory.git";
			TestSubject.CreateDir(dirName);
			Action readMissingFile = () => TestSubject.TextContents(dirName);
			readMissingFile.ShouldThrow<UnauthorizedAccessException>()
				.WithMessage(string.Format("Access to the path '{0}' is denied.", dirName));
		}

		[Test]
		public void StringsShouldBeEncodedInUtf8ByDefault()
		{
			var testFile = BaseFolder/"hello.txt";
			TestSubject.Overwrite(testFile, UnicodeContents);
			var asBytes = TestSubject.RawContents(testFile);
			asBytes.Should()
				.Equal(Encoding.UTF8.GetBytes(UnicodeContents));
		}

		[Test]
		public void BinaryFilesWithValidStringDataShouldBeReadableAsText()
		{
			var testFile = BaseFolder/"hello.txt";
			TestSubject.Overwrite(testFile, Encoding.UTF8.GetBytes(UnicodeContents));
			var asString = TestSubject.TextContents(testFile);
			asString.Should()
				.Be(UnicodeContents);
		}
	}

	public class CanReadAndWriteFileContentsAtomicallyDiskFs : CanReadAndWriteFileContentsAtomically
	{
		internal override _IFsDisk _MakeTestSubject()
		{
			return new _DiskReal();
		}
	}

	public class CanReadAndWriteFileContentsAtomicallyMemoryFs : CanReadAndWriteFileContentsAtomically
	{
		internal override _IFsDisk _MakeTestSubject()
		{
			return new _DiskSimulated();
		}
	}
}
