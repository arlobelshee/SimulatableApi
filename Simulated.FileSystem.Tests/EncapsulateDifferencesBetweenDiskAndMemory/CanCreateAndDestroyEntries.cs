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
			TestSubject.CreateDir(newPath)
				.RunAndWait();
			TestSubject.ShouldBeDir(newPath);
			TestSubject.ShouldNotBeFile(newPath);
		}

		[Test]
		public void CreatingNewDirectoryShouldCreateAllParents()
		{
			TestSubject.CreateDir(BaseFolder/"one"/"two"/"three")
				.RunAndWait();
			TestSubject.ShouldBeDir(BaseFolder/"one");
			TestSubject.ShouldBeDir(BaseFolder/"one"/"two");
			TestSubject.ShouldBeDir(BaseFolder/"one"/"two"/"three");
		}

		[Test]
		public void CreatingDirectoryWhichExistsShouldNoop()
		{
			var newPath = BaseFolder/"sub";
			var filePath = newPath/"file.txt";
			TestSubject.CreateDir(newPath)
				.RunAndWait();
			TestSubject.Overwrite(filePath, ArbitraryFileContents).RunAndWait();
			TestSubject.CreateDir(newPath)
				.RunAndWait();
			TestSubject.DirExistsNeedsToBeMadeDelayStart(newPath)
				.Should()
				.BeTrue();
			TestSubject.ShouldBeFile(filePath, ArbitraryFileContents);
		}

		[Test]
		public void CreatingNewDirectoryWhereFileExistsShouldFail()
		{
			var newPath = BaseFolder/"sub.txt";
			TestSubject.Overwrite(newPath, ArbitraryFileContents).RunAndWait();
			Action create = () => TestSubject.CreateDir(newPath)
				.RunAndWait();
			create.ShouldThrow<BadStorageRequest>()
				.WithMessage(string.Format(UserMessages.CreateErrorCreatedDirectoryOnTopOfFile, newPath));
		}

		[Test]
		public void DeletedDirectoryShouldNotExist()
		{
			var newPath = BaseFolder/"sub";
			TestSubject.CreateDir(newPath)
				.RunAndWait();
			TestSubject.ShouldBeDir(newPath);
			TestSubject.DeleteDir(newPath)
				.RunAndWait();
			TestSubject.ShouldNotExist(newPath);
		}

		[Test]
		public void DeletedFileShouldNotExist()
		{
			var newPath = BaseFolder/"sub.txt";
			TestSubject.Overwrite(newPath, ArbitraryFileContents).RunAndWait();
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

		[Test]
		public void UsingDeleteDirectoryOnAFileShouldFail()
		{
			var newPath = BaseFolder/"sub.txt";
			TestSubject.Overwrite(newPath, ArbitraryFileContents).RunAndWait();
			Action delete = () => TestSubject.DeleteDir(newPath)
				.RunAndWait();
			delete.ShouldThrow<BadStorageRequest>()
				.WithMessage(string.Format(UserMessages.DeleteErrorDeletedFileAsDirectory, newPath));
		}

		[Test]
		public void UsingDeleteFileOnADirectoryShouldFail()
		{
			var dirName = BaseFolder/"sub";
			TestSubject.CreateDir(dirName)
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
