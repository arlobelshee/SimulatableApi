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
			_testSubject.TempDirectory.Path.Should().Be(FsPath.TempFolder);
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
