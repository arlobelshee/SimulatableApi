using System;
using FluentAssertions;
using JetBrains.Annotations;
using NUnit.Framework;

namespace SimulatableApi.StreamStore.Tests.FileSystemNavigation
{
	public abstract class CanNavigateRelativeToAFileOrDirectory
	{
		[Test]
		public void CanGetTheParentOfADirectory()
		{
			_testSubject.Directory(@"C:\Base\Second").Parent.Path.Should().Be(new FsPath(@"C:\Base"));
		}

		[Test]
		public void CanGetAFileWithinADirectory()
		{
			_runRootFolder.File(ArbitraryFileName).FullPath.Should().Be(_runRootFolder.Path/ArbitraryFileName);
		}

		[Test]
		public void CanGetASubDirectory()
		{
			_runRootFolder.Dir(ArbitraryFileName).Path.Should().Be(_runRootFolder.Path/ArbitraryFileName);
		}

		[Test]
		public void AFileKnowsWhereItIs()
		{
			_runRootFolder.File(ArbitraryFileName).ContainingFolder.Should().Be(_runRootFolder);
		}

		[NotNull]
		protected abstract FileSystem MakeTestSubject();

		[NotNull] private FileSystem _testSubject;
		[NotNull] private FsDirectory _runRootFolder;

		[SetUp]
		public void Setup()
		{
			_testSubject = MakeTestSubject();
			_testSubject.EnableRevertToHere();
			_runRootFolder = _testSubject.TempDirectory.Dir("CreatedByTestRun-" + Guid.NewGuid());
		}

		[TearDown]
		public void Teardown()
		{
			_testSubject.RevertAllChanges();
		}

		private const string ArbitraryFileName = "something.txt";
	}

	[TestFixture]
	public class CanNavigateRelativeToAFileOrDirectoryRealFs : CanNavigateRelativeToAFileOrDirectory
	{
		protected override FileSystem MakeTestSubject()
		{
			return FileSystem.Real();
		}
	}

	[TestFixture]
	public class CanNavigateRelativeToAFileOrDirectoryMemoryFs : CanNavigateRelativeToAFileOrDirectory
	{
		protected override FileSystem MakeTestSubject()
		{
			return FileSystem.Simulated();
		}
	}
}
