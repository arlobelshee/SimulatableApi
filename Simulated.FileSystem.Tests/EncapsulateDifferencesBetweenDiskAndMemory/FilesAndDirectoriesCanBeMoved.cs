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
	public abstract class FilesAndDirectoriesCanBeMoved : DiskTestBase
	{
		[Test]
		public void MovingDirectoryShouldMoveAllContents()
		{
			var originalRoot = BaseFolder/"original";
			var newRoot = BaseFolder/"new";
			var filePath = originalRoot/"something"/"file.txt";
			TestSubject.Overwrite(filePath, ArbitraryFileContents);
			TestSubject.MoveDir(originalRoot, newRoot);
			TestSubject.ShouldBeDir(newRoot);
			TestSubject.ShouldNotExist(originalRoot);
			TestSubject.ShouldBeFile(newRoot/"something"/"file.txt", ArbitraryFileContents);
		}

		[Test]
		public void MovingFileShouldChangeItsLocation()
		{
			var originalFile = BaseFolder/"something"/"file.txt";
			var dest = BaseFolder / "new_path" / "file_new.txt";
			TestSubject.Overwrite(originalFile, ArbitraryFileContents);
			TestSubject.MoveFile(originalFile, dest);
			TestSubject.ShouldBeDir(BaseFolder / "new_path");
			TestSubject.ShouldNotExist(originalFile);
			TestSubject.ShouldBeFile(dest, ArbitraryFileContents);
		}

		[Test]
		public void UsingMoveDirectoryOnAFileShouldDenyAccess()
		{
			var fileName = BaseFolder/"sub.txt";
			TestSubject.Overwrite(fileName, ArbitraryFileContents);
			Action moveDir = () => TestSubject.MoveDir(fileName, BaseFolder/"dir");
			moveDir.ShouldThrow<UnauthorizedAccessException>()
				.WithMessage(string.Format("Cannot move the directory '{0}' because it is a file.", fileName));
		}

		[Test]
		public void UsingMoveFileOnADirectoryShouldFailToFindFile()
		{
			var dirName = BaseFolder/"sub";
			TestSubject.CreateDir(dirName);
			Action moveFile = () => TestSubject.MoveFile(dirName, BaseFolder/"dest.txt");
			moveFile.ShouldThrow<FileNotFoundException>()
				.WithMessage(string.Format("Could not find file '{0}'.", dirName));
		}

		[Test]
		public void MovingMissingFileShouldFailToFindFile()
		{
			var dirName = BaseFolder/"src.txt";
			Action moveFile = () => TestSubject.MoveFile(dirName, BaseFolder/"dest.txt");
			moveFile.ShouldThrow<FileNotFoundException>()
				.WithMessage(string.Format("Could not find file '{0}'.", dirName));
		}

		[Test]
		public void MovingMissingDirectoryShouldFailToFindDirectory()
		{
			Action moveDir = () => TestSubject.MoveDir(_src, _dest);
			moveDir.ShouldThrow<DirectoryNotFoundException>()
				.WithMessage(string.Format("Could not find a part of the path '{0}'.", _src));
		}

		[Test]
		public void MovingADirectoryToAnExistingFileShouldFail()
		{
			TestSubject.CreateDir(_src);
			TestSubject.Overwrite(_dest, ArbitraryFileContents);
			Action move = ()=>TestSubject.MoveDir(_src, _dest);
			move.ShouldThrow<BadStorageRequest>()
				.WithMessage(string.Format("Cannot move '{0}' to '{1}' because there is already something at the destination.", _src.Absolute, _dest.Absolute));
		}

		[Test]
		public void MovingADirectoryToAnExistingDirectoryShouldFail()
		{
			TestSubject.CreateDir(_src);
			TestSubject.CreateDir(_dest);
			Action move = ()=>TestSubject.MoveDir(_src, _dest);
			move.ShouldThrow<BadStorageRequest>()
				.WithMessage(string.Format("Cannot move '{0}' to '{1}' because there is already something at the destination.", _src.Absolute, _dest.Absolute));
		}

		[Test]
		public void MovingAFileToAnExistingFileShouldFail()
		{
			TestSubject.Overwrite(_src, ArbitraryFileContents);
			TestSubject.Overwrite(_dest, ArbitraryFileContents);
			Action move = ()=>TestSubject.MoveFile(_src, _dest);
			move.ShouldThrow<BadStorageRequest>()
				.WithMessage(string.Format("Cannot move '{0}' to '{1}' because there is already something at the destination.", _src.Absolute, _dest.Absolute));
		}

		[Test]
		public void MovingAFileToAnExistingDirectoryShouldFail()
		{
			TestSubject.Overwrite(_src, ArbitraryFileContents);
			TestSubject.CreateDir(_dest);
			Action move = ()=>TestSubject.MoveFile(_src, _dest);
			move.ShouldThrow<BadStorageRequest>()
				.WithMessage(string.Format("Cannot move '{0}' to '{1}' because there is already something at the destination.", _src.Absolute, _dest.Absolute));
		}

		protected override void FinishSetup()
		{
			_src = BaseFolder/"src";
			_dest = BaseFolder/"dest";
		}

		private FsPath _src;
		private FsPath _dest;
	}

	public class FilesAndDirectoriesCanBeMovedDiskFs : FilesAndDirectoriesCanBeMoved
	{
		internal override _IFsDisk _MakeTestSubject()
		{
			return new _DiskReal();
		}
	}

	public class FilesAndDirectoriesCanBeMovedMemoryFs : FilesAndDirectoriesCanBeMoved
	{
		internal override _IFsDisk _MakeTestSubject()
		{
			return new _DiskSimulated();
		}
	}
}
