﻿using System;
using System.IO;
using System.Text;
using FluentAssertions;
using JetBrains.Annotations;
using NUnit.Framework;
using Simulated.Tests.zzTestHelpers;

namespace Simulated.Tests.FileSystemModification
{
	public abstract class CanReadAndWriteFileContents
	{
		[Test]
		public void CanCreateFileAndReadItsContents()
		{
			_testFile.ShouldNotExist();
			_testFile.Overwrite(OriginalContents);
			_testFile.ShouldContain(OriginalContents);
		}

		[Test]
		public void RollingBackShouldRemoveAnyCreatedFiles()
		{
			_testFile.Overwrite(OriginalContents);

			_testFile.ShouldContain(OriginalContents);
			_testSubject.RevertAllChanges();
			_testFile.ShouldNotExist();
		}

		[Test]
		public void ChangingContentsOfFileShouldChangeContentsSeenByAllViews()
		{
			_testFile.Overwrite(OriginalContents);
			using (FileSystem secondView = _testSubject.Clone())
			{
				secondView.File(_testFile.FullPath).Overwrite(NewContents);
			}
			_testFile.ShouldContain(NewContents);
		}

		[Test]
		public void RevertingChangedFileContentsShouldReverToOriginalContents()
		{
			_testFile.Overwrite(OriginalContents);
			using (FileSystem secondView = _testSubject.Clone())
			{
				secondView.EnableRevertToHere();
				secondView.File(_testFile.FullPath).Overwrite(NewContents);
				_testFile.ShouldContain(NewContents);
			}
			_testFile.ShouldContain(OriginalContents);
		}

		[Test]
		public void CannotReadContentsOfMissingFile()
		{
			FsFile testFile = _testSubject.TempDirectory.File("CreatedByTest.txt");
			_Throws<FileNotFoundException>(() => testFile.ReadAllText(), string.Format("Could not find file '{0}'.", testFile.FullPath.Absolute));
		}

		[Test]
		public void CannotReadContentsOfFolder()
		{
			FsFile testFile = _testFile;
			_testSubject.Directory(testFile.FullPath).Create();
			_Throws<UnauthorizedAccessException>(() => testFile.ReadAllText(), string.Format("Access to the path '{0}' is denied.", testFile.FullPath.Absolute));
		}

		[Test]
		public void StringsShouldBeEncodedInUtf8ByDefault()
		{
			FsFile testFile = _testFile;
			testFile.Overwrite(NewContents);
			byte[] asString = testFile.ReadAllBytes();
			asString.Should().Equal(Encoding.UTF8.GetBytes(NewContents));
		}

		[Test]
		public void BinaryFilesWithValidStringDataShouldBeReadableAsText()
		{
			FsFile testFile = _testFile;
			testFile.OverwriteBinary(Encoding.UTF8.GetBytes(NewContents));
			string asString = testFile.ReadAllText();
			asString.Should().Be(NewContents);
		}

		private static void _Throws<TException>(TestDelegate code, string message) where TException : Exception
		{
			Assert.That(Assert.Throws<TException>(code), Has.Property("Message").EqualTo(message));
		}

		private const string OriginalContents = "Original contents";
		private const string NewContents = "helȽo ﺷ";
		[NotNull] private FileSystem _testSubject;
		[NotNull] private FsFile _testFile;

		[SetUp]
		public void Setup()
		{
			_testSubject = MakeTestSubject();
			_testSubject.EnableRevertToHere();
			var runRootFolder = _testSubject.TempDirectory.Dir("CreatedByTestRun-" + Guid.NewGuid());
			_testFile = runRootFolder.File("CreatedByTest.txt");
		}

		[TearDown]
		public void Teardown()
		{
			_testSubject.RevertAllChanges();
		}

		[NotNull]
		protected abstract FileSystem MakeTestSubject();
	}

	[TestFixture]
	public class CanReadAndWriteFileContentsInRealFs : CanReadAndWriteFileContents
	{
		protected override FileSystem MakeTestSubject()
		{
			return FileSystem.Real();
		}
	}

	[TestFixture]
	public class CanReadAndWriteFileContentsInMemoryFs : CanReadAndWriteFileContents
	{
		protected override FileSystem MakeTestSubject()
		{
			return FileSystem.Simulated();
		}
	}
}