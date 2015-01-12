// SimulatableAPI
// File: CanCreateAndDestroyEntries.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System;
using System.Threading.Tasks;
using FluentAssertions;
using JetBrains.Annotations;
using NUnit.Framework;
using Simulated.Tests.zzTestHelpers;
using Simulated._Fs;

namespace Simulated.Tests.EncapsulateDifferencesBetweenDiskAndMemory
{
	[TestFixture]
	public abstract class CanCreateAndDestroyEntries : DiskTestBase
	{
		[Test]
		public void NewDirectoryShouldExistWhenCreated()
		{
			var newPath = BaseFolder/"sub";
			TestSubject.ShouldNotExist(newPath);
			TestSubject.CreateDir(newPath).RunSynchronously();
			TestSubject.ShouldBeDir(newPath);
			TestSubject.ShouldNotBeFile(newPath);
		}

		[Test]
		public void CreatingNewDirectoryShouldCreateAllParents()
		{
			TestSubject.CreateDir(BaseFolder / "one" / "two" / "three").RunSynchronously();
			TestSubject.ShouldBeDir(BaseFolder/"one");
			TestSubject.ShouldBeDir(BaseFolder/"one"/"two");
			TestSubject.ShouldBeDir(BaseFolder/"one"/"two"/"three");
		}

		[Test]
		public async Task CreatingDirectoryWhichExistsShouldNoop()
		{
			var newPath = BaseFolder/"sub";
			var filePath = newPath/"file.txt";
			TestSubject.CreateDir(newPath).RunSynchronously();
			await TestSubject.OverwriteNeedsToBeMadeDelayStart(filePath, ArbitraryFileContents);
			TestSubject.CreateDir(newPath).RunSynchronously();
			TestSubject.DirExistsNeedsToBeMadeDelayStart(newPath)
				.Should()
				.BeTrue();
			TestSubject.ShouldBeFile(filePath, ArbitraryFileContents);
		}


		[NotNull,Test]
		public async Task CreatingNewDirectoryWhereFileExistsShouldFail()
		{
			var newPath = BaseFolder/"sub.txt";
			await TestSubject.OverwriteNeedsToBeMadeDelayStart(newPath, ArbitraryFileContents);
			Action create = () => TestSubject.CreateDir(newPath)
				.RunAndWait();
			create.ShouldThrow<BadStorageRequest>()
				.WithMessage(string.Format(UserMessages.CreateErrorCreatedDirectoryOnTopOfFile, newPath));
		}

		[Test]
		public void DeletedDirectoryShouldNotExist()
		{
			var newPath = BaseFolder/"sub";
			TestSubject.CreateDir(newPath).RunSynchronously();
			TestSubject.ShouldBeDir(newPath);
			TestSubject.DeleteDir(newPath).RunSynchronously();
			TestSubject.ShouldNotExist(newPath);
		}

		[NotNull]
		[Test]
		public async Task DeletedFileShouldNotExist()
		{
			var newPath = BaseFolder/"sub.txt";
			await TestSubject.OverwriteNeedsToBeMadeDelayStart(newPath, ArbitraryFileContents);
			TestSubject.DeleteFileNeedsToBeMadeDelayStart(newPath);
			TestSubject.ShouldNotExist(newPath);
		}

		[Test]
		public void DeletingMissingDirectoryShouldNoop()
		{
			var newPath = BaseFolder/"sub";
			TestSubject.ShouldNotExist(newPath);
			TestSubject.DeleteDir(newPath).RunSynchronously();
			TestSubject.ShouldNotExist(newPath);
		}

		[Test]
		public void DeletingMissingFileShouldNoop()
		{
			var newPath = BaseFolder/"sub.txt";
			TestSubject.ShouldNotExist(newPath);
			TestSubject.DeleteFileNeedsToBeMadeDelayStart(newPath);
			TestSubject.ShouldNotExist(newPath);
		}

		[Test]
		public async Task UsingDeleteDirectoryOnAFileShouldFail()
		{
			var newPath = BaseFolder/"sub.txt";
			await TestSubject.OverwriteNeedsToBeMadeDelayStart(newPath, ArbitraryFileContents);
			Action delete = () => TestSubject.DeleteDir(newPath).RunAndWait();
			delete.ShouldThrow<BadStorageRequest>()
				.WithMessage(string.Format(UserMessages.DeleteErrorDeletedFileAsDirectory, newPath));
		}

		[Test]
		public void UsingDeleteFileOnADirectoryShouldFail()
		{
			var dirName = BaseFolder/"sub";
			TestSubject.CreateDir(dirName).RunSynchronously();
			Action deleteFile = () => TestSubject.DeleteFileNeedsToBeMadeDelayStart(dirName);
			deleteFile.ShouldThrow<BadStorageRequest>()
				.WithMessage(string.Format("Failed to delete the file '{0}' because it is a directory.", dirName));
		}
	}

	public class CanCreateAndDestroyEntriesDiskFs : CanCreateAndDestroyEntries
	{
		internal override _IFsDisk _MakeTestSubject()
		{
			return new _DiskReal();
		}
	}

	public class CanCreateAndDestroyEntriesMemoryFs : CanCreateAndDestroyEntries
	{
		internal override _IFsDisk _MakeTestSubject()
		{
			return new _DiskSimulated();
		}
	}
}
