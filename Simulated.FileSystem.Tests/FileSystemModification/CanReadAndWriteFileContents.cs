// SimulatableAPI
// File: CanReadAndWriteFileContents.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using JetBrains.Annotations;
using NUnit.Framework;
using Simulated.Tests.zzTestHelpers;
using Simulated._Fs;

namespace Simulated.Tests.FileSystemModification
{
	public abstract class CanReadAndWriteFileContents : FileSystemTestBase
	{
		[Test]
		public async Task CanCreateFileAndReadItsContents()
		{
			_testFile.ShouldNotExist();
			await _testFile.Overwrite(OriginalContents);
			_testFile.ShouldContain(OriginalContents);
		}

		[Test]
		public async Task RollingBackShouldRemoveAnyCreatedFiles()
		{
			await _testFile.Overwrite(OriginalContents);

			_testFile.ShouldContain(OriginalContents);
			_testSubject.RevertAllChanges();
			_testFile.ShouldNotExist();
		}

		[Test]
		public async Task ChangingContentsOfFileShouldChangeContentsSeenByAllViews()
		{
			await _testFile.Overwrite(OriginalContents);
			using (var secondView = _testSubject.Clone())
			{
				await secondView.File(_testFile.FullPath)
					.Overwrite(NewContents);
			}
			_testFile.ShouldContain(NewContents);
		}

		[Test]
		public async Task CommittingChangesShouldCompletelyEliminateAllUndoData()
		{
			await _testFile.Overwrite(OriginalContents);
			using (var secondView = _testSubject.Clone())
			{
				secondView.EnableRevertToHere();
				await secondView.File(_testFile.FullPath)
					.Overwrite(NewContents);
				var originalDataCache = await secondView._UndoDataCache();

				(await originalDataCache.Files("*.*"))
					.Should()
					.NotBeEmpty();
				secondView.CommitChanges();
				(await originalDataCache.Files("*.*"))
					.Should()
					.BeEquivalentTo();
				originalDataCache.ShouldNotExist();
			}
		}

		[Test]
		public async Task RollingBackChangesShouldCompletelyEliminateAllUndoData()
		{
			await _testFile.Overwrite(OriginalContents);
			using (var secondView = _testSubject.Clone())
			{
				secondView.EnableRevertToHere();
				await secondView.File(_testFile.FullPath)
					.Overwrite(NewContents);
				var originalDataCache = await secondView._UndoDataCache();

				(await originalDataCache.Files("*.*"))
					.Should()
					.NotBeEmpty();
				secondView.RevertAllChanges();
				(await originalDataCache.Files("*.*"))
					.Should()
					.BeEquivalentTo();
				originalDataCache.ShouldNotExist();
			}
		}

		[Test]
		public async Task RevertingChangedFileContentsShouldRevertToOriginalContents()
		{
			await _testFile.Overwrite(OriginalContents);
			using (var secondView = _testSubject.Clone())
			{
				secondView.EnableRevertToHere();
				await secondView.File(_testFile.FullPath)
					.Overwrite(NewContents);
				_testFile.ShouldContain(NewContents);
			}
			_testFile.ShouldContain(OriginalContents);
		}

		[Test]
		public async Task CannotReadContentsOfMissingFile()
		{
			var testFile = (await _testSubject.TempDirectory).File("CreatedByTest.txt");
			_Throws<FileNotFoundException>(() => testFile.ReadAllText(), string.Format("Could not find file '{0}'.", testFile.FullPath.Absolute));
		}

		[Test]
		public async Task CannotReadContentsOfFolder()
		{
			var testFile = _testFile;
			await _testSubject.Directory(testFile.FullPath)
				.EnsureExists();
			_Throws<UnauthorizedAccessException>(() => testFile.ReadAllText(), string.Format("Access to the path '{0}' is denied.", testFile.FullPath.Absolute));
		}

		[Test]
		public async Task StringsShouldBeEncodedInUtf8ByDefault()
		{
			var testFile = _testFile;
			await testFile.Overwrite(NewContents);
			var asString = testFile.ReadAllBytes();
			asString.Should()
				.Equal(Encoding.UTF8.GetBytes(NewContents));
		}

		[Test]
		public async Task BinaryFilesWithValidStringDataShouldBeReadableAsText()
		{
			var testFile = _testFile;
			await testFile.OverwriteBinary(Encoding.UTF8.GetBytes(NewContents));
			var asString = testFile.ReadAllText();
			asString.Should()
				.Be(NewContents);
		}

		private static void _Throws<TException>(TestDelegate code, string message) where TException : Exception
		{
			Assert.That(Assert.Throws<TException>(code), Has.Property("Message")
				.EqualTo(message));
		}

		private const string OriginalContents = "Original contents";
		private const string NewContents = "helȽo ﺷ";
		[NotNull] private FsFile _testFile;

		protected override void FinishSetup()
		{
			_testFile = _runRootFolder.File("CreatedByTest.txt");
		}
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
