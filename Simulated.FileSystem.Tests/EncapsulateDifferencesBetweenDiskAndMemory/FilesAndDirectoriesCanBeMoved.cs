// SimulatableAPI
// File: FilesAndDirectoriesCanBeMoved.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System;
using System.IO;
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
			var dest = BaseFolder/"new_path"/"file_new.txt";
			TestSubject.Overwrite(originalFile, ArbitraryFileContents);
			TestSubject.MoveFile(originalFile, dest);
			TestSubject.ShouldBeDir(BaseFolder/"new_path");
			TestSubject.ShouldNotExist(originalFile);
			TestSubject.ShouldBeFile(dest, ArbitraryFileContents);
		}

		[Test]
		[TestCaseSource("ErrorCases")]
		public void AllMoveErrorCasesShouldBeConsistentAndInformative(MoveKind operationToAttempt, [NotNull] string srcName, [NotNull] string destName,
			[NotNull] string expectedError)
		{
			var src = BaseFolder/srcName;
			var dest = BaseFolder/destName;
			Action move = () =>
			{
				switch (operationToAttempt)
				{
					case MoveKind.Directory:
						TestSubject.MoveDir(src, dest);
						break;
					case MoveKind.File:
						TestSubject.MoveFile(src, dest);
						break;
					default:
						throw new NotImplementedException(string.Format("Test not yet written for operation {0}.", operationToAttempt));
				}
			};
			move.ShouldThrow<BadStorageRequest>()
				.WithMessage(string.Format(expectedError, src.Absolute, dest.Absolute));
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

		private const string DestBlockingDir = "dest_blocking_dir";
		private const string DestBlockingFile = "dest_blocking_file.txt";
		private const string DestUnblocked = "dest_new";

		[NotNull]
		public object[][] ErrorCases
		{
			get
			{
				return new[]
				{
					new object[] {MoveKind.Directory, SrcDir, DestBlockingDir, UserMessages.MoveErrorDestinationBlocked},
					new object[] {MoveKind.Directory, SrcDir, DestBlockingFile, UserMessages.MoveErrorDestinationBlocked},
					new object[] {MoveKind.Directory, SrcMissing, DestUnblocked, UserMessages.MoveErrorMissingSource},
					new object[] {MoveKind.File, SrcFile, DestBlockingDir, UserMessages.MoveErrorDestinationBlocked},
					new object[] {MoveKind.File, SrcFile, DestBlockingFile, UserMessages.MoveErrorDestinationBlocked}
				};
			}
		}

		protected override void FinishSetup()
		{
			TestSubject.Overwrite(BaseFolder/SrcFile, ArbitraryFileContents);
			TestSubject.CreateDir(BaseFolder/SrcDir);

			TestSubject.Overwrite(BaseFolder/DestBlockingFile, ArbitraryFileContents);
			TestSubject.CreateDir(BaseFolder/DestBlockingDir);
		}

		public enum MoveKind
		{
			Directory,
			File
		}

		private const string SrcDir = "src_dir";
		private const string SrcFile = "src_file.txt";
		private const string SrcMissing = "src_missing";
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
