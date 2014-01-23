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
	public abstract class DirectoryAndFileOperations
	{
		private const string ArbitraryFileContents = "contents";
		private FsPath _baseFolder;
		private _IFsDisk _testSubject;

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
		public void FileMatchingShouldMatchStarPatterns(string searchPattern, params string[] expectedMatches)
		{
			_testSubject.Overwrite(_baseFolder/"matches.txt", ArbitraryFileContents);
			_testSubject.Overwrite(_baseFolder/"matches.jpg", ArbitraryFileContents);
			_testSubject.Overwrite(_baseFolder/"no_match.txt", ArbitraryFileContents);
			_testSubject.FindFiles(_baseFolder, searchPattern)
				.Should()
				.BeEquivalentTo(expectedMatches.Select(m => _baseFolder/m));
		}

		[Test]
		public void DeletedDirectoryShouldNotExist()
		{
			var newPath = _baseFolder/"sub";
			_testSubject.CreateDir(newPath);
			_testSubject.DirExists(newPath)
				.Should()
				.BeTrue();
			_testSubject.DeleteDir(newPath);
			_testSubject.DirExists(newPath)
				.Should()
				.BeFalse();
		}

		[Test]
		public void DeletingMissingDirectoryShouldNoop()
		{
			var newPath = _baseFolder/"sub";
			_testSubject.DirExists(newPath)
				.Should()
				.BeFalse();
			_testSubject.DeleteDir(newPath);
			_testSubject.DirExists(newPath)
				.Should()
				.BeFalse();
		}

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

		[NotNull]
		internal abstract _IFsDisk _MakeTestSubject();
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
