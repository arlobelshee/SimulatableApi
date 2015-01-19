// SimulatableAPI
// File: CanReadAndWriteFileContentsAtomically.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using JetBrains.Annotations;
using NUnit.Framework;
using Simulated.Tests.zzTestHelpers;
using Simulated._Fs;

namespace Simulated.Tests.EncapsulateDifferencesBetweenDiskAndMemory
{
	[TestFixture]
	public abstract class CanReadAndWriteFileContentsAtomically : DiskTestBase
	{
		[NotNull]
		[Test]
		public async Task CanCreateFileAndReadItsContents()
		{
			var fileName = BaseFolder/"file.txt";
			TestSubject.ShouldNotExist(fileName);
			await TestSubject.Overwrite(fileName, ArbitraryFileContents);
			TestSubject.ShouldBeFile(fileName, ArbitraryFileContents);
		}

		[Test]
		public void CanCreateBinaryFileAndReadItsContents()
		{
			var fileName = BaseFolder/"file.txt";
			var contents = Encoding.UTF8.GetBytes(ArbitraryFileContents);
			TestSubject.ShouldNotExist(fileName);
			TestSubject.Overwrite(fileName, contents);
			TestSubject.ShouldBeFile(fileName, contents);
		}

		[NotNull,Test]
		[TestCaseSource("FileFormats")]
		public async Task WritingToFileInMissingDirectoryShouldCreateParentDirs(FileFormat fileFormat)
		{
			var fileName = BaseFolder/"parent"/"file.txt";
			var writeToFile = _PickFileWriter(fileFormat, fileName, ArbitraryFileContents);
			await writeToFile();
			TestSubject.ShouldBeDir(BaseFolder/"parent");
		}

		[NotNull,Test]
		[TestCaseSource("FileFormats")]
		public async Task WritingToFileWhereDirectoryExistsShouldFail(FileFormat fileFormat)
		{
			var fileName = BaseFolder/"parent"/"file.txt";
			await TestSubject.CreateDir(fileName);
			var writeToFile = _PickFileWriter(fileFormat, fileName, ArbitraryFileContents);
			writeToFile.ShouldThrow<BadStorageRequest>()
				.WithMessage(string.Format(UserMessages.WriteErrorPathIsDirectory, fileName._Absolute));
		}

		[Test]
		[TestCaseSource("FileFormats")]
		public void CannotReadContentsOfMissingFile(FileFormat fileFormat)
		{
			var missingFileName = BaseFolder/"missing.txt";
			var readMissingFile = _PickFileReader(fileFormat, missingFileName);
			readMissingFile.ShouldThrow<BadStorageRequest>()
				.WithMessage(string.Format(UserMessages.ReadErrorFileNotFound, missingFileName));
		}

		[NotNull,Test]
		[TestCaseSource("FileFormats")]
		public async Task CannotReadContentsOfFolder(FileFormat fileFormat)
		{
			var dirName = BaseFolder/"directory.git";
			await TestSubject.CreateDir(dirName);
			var readMissingFile = _PickFileReader(fileFormat, dirName);
			readMissingFile.ShouldThrow<BadStorageRequest>()
				.WithMessage(string.Format(UserMessages.ReadErrorPathIsDirectory, dirName));
		}

		[NotNull]
		[Test]
		public async Task StringsShouldBeEncodedInUtf8ByDefault()
		{
			var testFile = BaseFolder/"hello.txt";
			await TestSubject.Overwrite(testFile, UnicodeContents);
			var asBytes = TestSubject.RawContents(testFile)
				.CollectAllBytes();
			asBytes.Should()
				.Equal(Encoding.UTF8.GetBytes(UnicodeContents));
		}

		[Test]
		public void BinaryFilesWithValidStringDataShouldBeReadableAsText()
		{
			var testFile = BaseFolder/"hello.txt";
			TestSubject.Overwrite(testFile, Encoding.UTF8.GetBytes(UnicodeContents));
			var asString = TestSubject.TextContents(testFile)
				.Result;
			asString.Should()
				.Be(UnicodeContents);
		}

		public enum FileFormat
		{
			Text,
			Binary
		}

		[NotNull]
		public object[][] FileFormats
		{
			get { return new[] {new object[] {FileFormat.Binary}, new object[] {FileFormat.Text}}; }
		}

		[NotNull]
		private Action _PickFileReader(FileFormat fileFormat, [NotNull] FsPath fileName)
		{
			switch (fileFormat)
			{
				case FileFormat.Text:
					return () => TestSubject.TextContents(fileName)
						.Wait();
				case FileFormat.Binary:
					return () => TestSubject.RawContents(fileName)
						.Wait();
				default:
					throw new NotImplementedException(string.Format("Test does not support {0}.", fileFormat));
			}
		}

		[NotNull]
		private Func<Task> _PickFileWriter(FileFormat fileFormat, [NotNull] FsPath fileName, [NotNull] string contents)
		{
			switch (fileFormat)
			{
				case FileFormat.Text:
					return () => TestSubject.Overwrite(fileName, contents);
				case FileFormat.Binary:
					return () => TestSubject.Overwrite(fileName, Encoding.UTF8.GetBytes(contents));
				default:
					throw new NotImplementedException(string.Format("Test does not support {0}.", fileFormat));
			}
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
