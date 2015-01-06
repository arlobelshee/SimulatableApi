// SimulatableAPI
// File: FilesAndDirectoriesCanBeMoved.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System;
using System.IO;
using System.Threading.Tasks;
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
		[NotNull,Test]
		public async Task MovingDirectoryShouldMoveAllContents()
		{
			var originalRoot = BaseFolder/"original";
			var newRoot = BaseFolder/"new";
			var filePath = originalRoot/"something"/"file.txt";
			await TestSubject.OverwriteNeedsToBeMadeDelayStart(filePath, ArbitraryFileContents);
			TestSubject.MoveDirNeedsToBeMadeDelayStart(originalRoot, newRoot);
			TestSubject.ShouldBeDir(newRoot);
			TestSubject.ShouldNotExist(originalRoot);
			TestSubject.ShouldBeFile(newRoot/"something"/"file.txt", ArbitraryFileContents);
		}

		[NotNull,Test]
		public async Task MovingFileShouldChangeItsLocation()
		{
			var originalFile = BaseFolder/"something"/"file.txt";
			var dest = BaseFolder/"new_path"/"file_new.txt";
			await TestSubject.OverwriteNeedsToBeMadeDelayStart(originalFile, ArbitraryFileContents);
			TestSubject.MoveFileNeedsToBeMadeDelayStart(originalFile, dest);
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
						TestSubject.MoveDirNeedsToBeMadeDelayStart(src, dest);
						break;
					case MoveKind.File:
						TestSubject.MoveFileNeedsToBeMadeDelayStart(src, dest);
						break;
					default:
						throw new NotImplementedException(string.Format("Test not yet written for operation {0}.", operationToAttempt));
				}
			};
			move.ShouldThrow<BadStorageRequest>()
				.WithMessage(string.Format(expectedError, src._Absolute, dest._Absolute));
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
					new object[] {MoveKind.Directory, SrcMissing, DestUnblocked, UserMessages.MoveErrorMissingSource},
					new object[] {MoveKind.Directory, SrcFile, DestUnblocked, UserMessages.MoveErrorMovedFileAsDirectory},
					new object[] {MoveKind.Directory, SrcDir, DestBlockingDir, UserMessages.MoveErrorDestinationBlocked},
					new object[] {MoveKind.Directory, SrcDir, DestBlockingFile, UserMessages.MoveErrorDestinationBlocked},
					new object[] {MoveKind.File, SrcMissing, DestUnblocked, UserMessages.MoveErrorMissingSource},
					new object[] {MoveKind.File, SrcDir, DestUnblocked, UserMessages.MoveErrorMovedDirectoryAsFile},
					new object[] {MoveKind.File, SrcFile, DestBlockingDir, UserMessages.MoveErrorDestinationBlocked},
					new object[] {MoveKind.File, SrcFile, DestBlockingFile, UserMessages.MoveErrorDestinationBlocked}
				};
			}
		}

		protected override void FinishSetup()
		{
			TestSubject.OverwriteNeedsToBeMadeDelayStart(BaseFolder/SrcFile, ArbitraryFileContents).Wait();
			TestSubject.CreateDir(BaseFolder/SrcDir).RunSynchronously();

			TestSubject.OverwriteNeedsToBeMadeDelayStart(BaseFolder/DestBlockingFile, ArbitraryFileContents).Wait();
			TestSubject.CreateDir(BaseFolder/DestBlockingDir).RunSynchronously();
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
