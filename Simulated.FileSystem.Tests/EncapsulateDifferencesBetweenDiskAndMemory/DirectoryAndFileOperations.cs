// SimulatableAPI
// File: DirectoryAndFileOperations.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System;
using System.Linq;
using FluentAssertions;
using JetBrains.Annotations;
using NUnit.Framework;
using Simulated.Tests.zzTestHelpers;
using Simulated._Fs;

namespace Simulated.Tests.EncapsulateDifferencesBetweenDiskAndMemory
{
	[TestFixture]
	public abstract class DirectoryAndFileOperations : DiskTestBase
	{
		[Test]
		public void NewDirectoryShouldExistWhenCreated()
		{
			var newPath = BaseFolder/"sub";
			TestSubject.DirExists(newPath)
				.Should()
				.BeFalse();
			TestSubject.CreateDir(newPath);
			TestSubject.DirExists(newPath)
				.Should()
				.BeTrue();
			TestSubject.FileExists(newPath)
				.Should()
				.BeFalse();
		}

		[Test]
		public void CreatingNewDirectoryShouldCreateAllParents()
		{
			TestSubject.CreateDir(BaseFolder/"one"/"two"/"three");
			TestSubject.ShouldBeDir(BaseFolder/"one");
			TestSubject.ShouldBeDir(BaseFolder/"one"/"two");
			TestSubject.ShouldBeDir(BaseFolder/"one"/"two"/"three");
		}

		[Test]
		public void CreatingDirectoryWhichExistsShouldNoop()
		{
			var newPath = BaseFolder/"sub";
			var filePath = newPath/"file.txt";
			TestSubject.CreateDir(newPath);
			TestSubject.Overwrite(filePath, ArbitraryFileContents);
			TestSubject.CreateDir(newPath);
			TestSubject.DirExists(newPath)
				.Should()
				.BeTrue();
			TestSubject.ShouldBeFile(filePath, ArbitraryFileContents);
		}

		[Test]
		[TestCase("matches.*", new[] {"matches.txt", "matches.jpg"})]
		[TestCase("*.*", new[] {"matches.txt", "matches.jpg", "no_match.txt"})]
		[TestCase("*.txt", new[] {"matches.txt", "no_match.txt"})]
		[TestCase("matches.txt", new[] {"matches.txt"})]
		public void FileMatchingShouldMatchStarPatterns([NotNull] string searchPattern, [NotNull] string[] expectedMatches)
		{
			TestSubject.Overwrite(BaseFolder/"matches.txt", ArbitraryFileContents);
			TestSubject.Overwrite(BaseFolder/"matches.jpg", ArbitraryFileContents);
			TestSubject.Overwrite(BaseFolder/"no_match.txt", ArbitraryFileContents);
			TestSubject.FindFiles(BaseFolder, searchPattern)
				.Should()
				.BeEquivalentTo(expectedMatches.Select(m => BaseFolder/m));
		}

		[Test]
		public void DeletedDirectoryShouldNotExist()
		{
			var newPath = BaseFolder/"sub";
			TestSubject.CreateDir(newPath);
			TestSubject.ShouldBeDir(newPath);
			TestSubject.DeleteDir(newPath);
			TestSubject.ShouldNotExist(newPath);
		}

		[Test]
		public void DeletedFileShouldNotExist()
		{
			var newPath = BaseFolder/"sub.txt";
			TestSubject.Overwrite(newPath, ArbitraryFileContents);
			TestSubject.DeleteFile(newPath);
			TestSubject.ShouldNotExist(newPath);
		}

		[Test]
		public void DeletingMissingDirectoryShouldNoop()
		{
			var newPath = BaseFolder/"sub";
			TestSubject.ShouldNotExist(newPath);
			TestSubject.DeleteDir(newPath);
			TestSubject.ShouldNotExist(newPath);
		}

		[Test]
		public void DeletingMissingFileShouldNoop()
		{
			var newPath = BaseFolder/"sub.txt";
			TestSubject.ShouldNotExist(newPath);
			TestSubject.DeleteFile(newPath);
			TestSubject.ShouldNotExist(newPath);
		}

		[Test]
		public void UsingDeleteDirectoryOnAFileShouldNoOp()
		{
			var newPath = BaseFolder/"sub.txt";
			TestSubject.Overwrite(newPath, ArbitraryFileContents);
			TestSubject.DeleteDir(newPath);
			TestSubject.ShouldBeFile(newPath, ArbitraryFileContents);
		}

		[Test]
		public void UsingDeleteFileOnADirectoryShouldRefuseAccess()
		{
			var dirName = BaseFolder/"sub";
			TestSubject.CreateDir(dirName);
			Action deleteFile = () => TestSubject.DeleteFile(dirName);
			deleteFile.ShouldThrow<UnauthorizedAccessException>()
				.WithMessage(string.Format("Access to the path '{0}' is denied.", dirName));
		}
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
