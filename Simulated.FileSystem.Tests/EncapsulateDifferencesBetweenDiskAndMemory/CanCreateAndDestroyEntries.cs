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
			TestSubject.CreateDirReturnsNonStartedTask(newPath)
				.RunAndWait();
			TestSubject.ShouldBeDir(newPath);
			TestSubject.ShouldNotBeFile(newPath);
		}

		[Test]
		public void CreatingNewDirectoryShouldCreateAllParents()
		{
			TestSubject.CreateDirReturnsNonStartedTask(BaseFolder/"one"/"two"/"three")
				.RunAndWait();
			TestSubject.ShouldBeDir(BaseFolder/"one");
			TestSubject.ShouldBeDir(BaseFolder/"one"/"two");
			TestSubject.ShouldBeDir(BaseFolder/"one"/"two"/"three");
		}

		[NotNull]
		[Test]
		public async Task CreatingDirectoryWhichExistsShouldNoop()
		{
			var newPath = BaseFolder/"sub";
			var filePath = newPath/"file.txt";
			TestSubject.CreateDirReturnsNonStartedTask(newPath)
				.RunAndWait();
			await TestSubject.Overwrite(filePath, ArbitraryFileContents);
			TestSubject.CreateDirReturnsNonStartedTask(newPath)
				.RunAndWait();
			TestSubject.DirExistsNeedsToBeMadeDelayStart(newPath)
				.Should()
				.BeTrue();
			TestSubject.ShouldBeFile(filePath, ArbitraryFileContents);
		}

		[NotNull]
		[Test]
		public async Task CreatingNewDirectoryWhereFileExistsShouldFail()
		{
			var newPath = BaseFolder/"sub.txt";
			await TestSubject.Overwrite(newPath, ArbitraryFileContents);
			Action create = () => TestSubject.CreateDirReturnsNonStartedTask(newPath)
				.RunAndWait();
			create.ShouldThrow<BadStorageRequest>()
				.WithMessage(string.Format(UserMessages.CreateErrorCreatedDirectoryOnTopOfFile, newPath));
		}

		[Test]
		public void DeletedDirectoryShouldNotExist()
		{
			var newPath = BaseFolder/"sub";
			TestSubject.CreateDirReturnsNonStartedTask(newPath)
				.RunAndWait();
			TestSubject.ShouldBeDir(newPath);
			TestSubject.DeleteDir(newPath)
				.RunAndWait();
			TestSubject.ShouldNotExist(newPath);
		}

		[NotNull]
		[Test]
		public async Task DeletedFileShouldNotExist()
		{
			var newPath = BaseFolder/"sub.txt";
			await TestSubject.Overwrite(newPath, ArbitraryFileContents);
			TestSubject.DeleteFileNeedsToBeMadeDelayStart(newPath);
			TestSubject.ShouldNotExist(newPath);
		}

		[Test]
		public void DeletingMissingDirectoryShouldNoop()
		{
			var newPath = BaseFolder/"sub";
			TestSubject.ShouldNotExist(newPath);
			TestSubject.DeleteDir(newPath)
				.RunAndWait();
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

		[NotNull]
		[Test]
		public async Task UsingDeleteDirectoryOnAFileShouldFail()
		{
			var newPath = BaseFolder/"sub.txt";
			await TestSubject.Overwrite(newPath, ArbitraryFileContents);
			Action delete = () => TestSubject.DeleteDir(newPath)
				.RunAndWait();
			delete.ShouldThrow<BadStorageRequest>()
				.WithMessage(string.Format(UserMessages.DeleteErrorDeletedFileAsDirectory, newPath));
		}

		[Test]
		public void UsingDeleteFileOnADirectoryShouldFail()
		{
			var dirName = BaseFolder/"sub";
			TestSubject.CreateDirReturnsNonStartedTask(dirName)
				.RunAndWait();
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
