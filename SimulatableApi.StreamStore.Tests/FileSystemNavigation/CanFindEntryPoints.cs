using System;
using System.IO;
using FluentAssertions;
using JetBrains.Annotations;
using NUnit.Framework;

namespace SimulatableApi.StreamStore.Tests.FileSystemNavigation
{
	public abstract class CanFindEntryPoints
	{
		[Test]
		public void TempFolderShouldHaveTheCorrectPath()
		{
			string tempPath = Path.GetTempPath();
			tempPath = tempPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

			_testSubject.TempDirectory.Path.Absolute.Should().Be(tempPath);
		}

		[Test]
		public void TempFolderShouldInitiallyExist()
		{
			_testSubject.TempDirectory.Exists.Should().BeTrue();
		}

		[Test]
		public void ShouldBeAbleToMakeReferenceToAbsolutePath()
		{
			Assert.That(_testSubject.Directory(ArbitraryMissingFolder).Path.Absolute, Is.EqualTo(ArbitraryMissingFolder));
		}

		[Test]
		public void ShouldRejectEmptyPathToDirectory()
		{
			_Throws<ArgumentNullException>(() => _testSubject.Directory(string.Empty),
				"A path cannot be null or empty.\r\nParameter name: absolutePath");
		}

		[Test]
		public void ShouldRejectTryingToLocateDirectoryAPrioriWithNonAbsolutePath()
		{
			_Throws<ArgumentException>(() => _testSubject.Directory(@"something\relative"),
				"The path must be absolute. 'something\\relative' is not an absolute path.\r\nParameter name: absolutePath");
		}

		[Test]
		public void ShouldRejectEmptyPathToFile()
		{
			_Throws<ArgumentNullException>(() => _testSubject.File(string.Empty),
				"A path cannot be null or empty.\r\nParameter name: absolutePath");
		}

		[Test]
		public void ShouldRejectTryingToLocateFileAPrioriWithNonAbsolutePath()
		{
			_Throws<ArgumentException>(() => _testSubject.File(@"something\relative.txt"),
				"The path must be absolute. 'something\\relative.txt' is not an absolute path.\r\nParameter name: absolutePath");
		}

		[Test]
		public void DirectoriesAreTheSameWhetherCreatedWithTrailingSlashOrNot()
		{
			Assert.That(_testSubject.Directory(@"C:\Path\").Path.Absolute, Is.EqualTo(@"C:\Path"));
		}

		[NotNull] private FileSystem _testSubject;

		[SetUp]
		public void Setup()
		{
			_testSubject = MakeTestSubject();
			_testSubject.EnableRevertToHere();
		}

		[TearDown]
		public void Teardown()
		{
			_testSubject.RevertAllChanges();
		}

		[NotNull]
		protected abstract FileSystem MakeTestSubject();

		private static void _Throws<TException>(Action code, string message) where TException : Exception
		{
			code.ShouldThrow<TException>().WithMessage(message);
		}

		private const string ArbitraryMissingFolder = @"C:\theroot\folder";
	}

	[TestFixture]
	public class CanFindEntryPointsMemoryFs : CanFindEntryPoints
	{
		protected override FileSystem MakeTestSubject()
		{
			return FileSystem.Simulated();
		}
	}

	[TestFixture]
	public class CanFindEntryPointsRealFs : CanFindEntryPoints
	{
		protected override FileSystem MakeTestSubject()
		{
			return FileSystem.Real();
		}
	}
}
