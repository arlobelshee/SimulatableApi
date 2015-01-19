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
		[NotNull]
		[Test]
		public async Task NewDirectoryShouldExistWhenCreated()
		{
			var newPath = BaseFolder/"sub";
			TestSubject.ShouldNotExist(newPath);
			await TestSubject.CreateDir(newPath);
			TestSubject.ShouldBeDir(newPath);
			TestSubject.ShouldNotBeFile(newPath);
		}

		[NotNull]
		[Test]
		public async Task CreatingNewDirectoryShouldCreateAllParents()
		{
			await TestSubject.CreateDir(BaseFolder/"one"/"two"/"three");
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
			await TestSubject.Overwrite(filePath, ArbitraryFileContents);
			await TestSubject.CreateDir(newPath);
			TestSubject.ShouldBeDir(newPath);
			TestSubject.ShouldBeFile(filePath, ArbitraryFileContents);
		}

		[NotNull]
		[Test]
		public async Task CreatingNewDirectoryWhereFileExistsShouldFail()
		{
			var newPath = BaseFolder/"sub.txt";
			await TestSubject.Overwrite(newPath, ArbitraryFileContents);
			Action create = () => TestSubject.CreateDir(newPath)
				.Wait();
			create.ShouldThrow<BadStorageRequest>()
				.WithMessage(string.Format(UserMessages.CreateErrorCreatedDirectoryOnTopOfFile, newPath));
		}

		[NotNull]
		[Test]
		public async Task DeletedDirectoryShouldNotExist()
		{
			var newPath = BaseFolder/"sub";
			await TestSubject.CreateDir(newPath);
			TestSubject.ShouldBeDir(newPath);
			await TestSubject.DeleteDir(newPath);
			TestSubject.ShouldNotExist(newPath);
		}

		[NotNull]
		[Test]
		public async Task DeletedFileShouldNotExist()
		{
			var newPath = BaseFolder/"sub.txt";
			await TestSubject.Overwrite(newPath, ArbitraryFileContents);
			await TestSubject.DeleteFile(newPath);
			TestSubject.ShouldNotExist(newPath);
		}

		[NotNull,Test]
		public async Task DeletingMissingDirectoryShouldNoop()
		{
			var newPath = BaseFolder/"sub";
			TestSubject.ShouldNotExist(newPath);
			await TestSubject.DeleteDir(newPath);
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

		[NotNull]
		[Test]
		public async Task UsingDeleteDirectoryOnAFileShouldFail()
		{
			var newPath = BaseFolder/"sub.txt";
			await TestSubject.Overwrite(newPath, ArbitraryFileContents);
			Action delete = () => TestSubject.DeleteDir(newPath)
				.Wait();
			delete.ShouldThrow<BadStorageRequest>()
				.WithMessage(string.Format(UserMessages.DeleteErrorDeletedFileAsDirectory, newPath));
		}

		[NotNull,Test]
		public async Task UsingDeleteFileOnADirectoryShouldFail()
		{
			var dirName = BaseFolder/"sub";
			await TestSubject.CreateDir(dirName);
			Action deleteFile = () => TestSubject.DeleteFile(dirName).Wait();
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
