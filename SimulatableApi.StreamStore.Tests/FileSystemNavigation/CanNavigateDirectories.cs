﻿using System;
using System.IO;
using FluentAssertions;
using JetBrains.Annotations;
using NUnit.Framework;

namespace SimulatableApi.StreamStore.Tests.FileSystemNavigation
{
	[TestFixture]
	public class CanNavigateDirectories_RealFs
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
		public void ShouldRejectEmptyDirectory()
		{
			_Throws<ArgumentNullException>(() => _testSubject.Directory(string.Empty), "A path cannot be null or empty.\r\nParameter name: absolutePath");
		}

		[Test]
		public void ShouldRejectRelativePathWithoutBase()
		{
			_Throws<ArgumentException>(() => _testSubject.Directory(@"something\relative"),
				"The path must be absolute. 'something\\relative' is not an absolute path.\r\nParameter name: absolutePath");
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
		protected virtual FileSystem MakeTestSubject()
		{
			return FileSystem.Real();
		}

		private static void _Throws<TException>(Action code, string message) where TException : Exception
		{
			code.ShouldThrow<TException>().WithMessage(message);
		}

		private const string ArbitraryMissingFolder = @"C:\theroot\folder";
	}

	[TestFixture]
	public class CanNavigateDirectories_MemoryFs : CanNavigateDirectories_RealFs
	{
		protected override FileSystem MakeTestSubject()
		{
			return FileSystem.Simulated();
		}
	}
}