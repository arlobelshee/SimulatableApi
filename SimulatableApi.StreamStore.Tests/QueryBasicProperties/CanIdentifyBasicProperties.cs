using System;
using FluentAssertions;
using JetBrains.Annotations;
using NUnit.Framework;

namespace SimulatableApi.StreamStore.Tests.QueryBasicProperties
{
	public abstract class CanIdentifyBasicProperties
	{
		[Test]
		public void AFileKnowsItsFileNameParts()
		{
			const string fileName = "ArbitraryFile.txt";
			const string extension = ".txt";

			FsFile f = _runRootFolder.File(fileName);
			f.FileName.Should().Be(fileName);
			f.Extension.Should().Be(extension);
		}

		[NotNull]
		protected abstract FileSystem MakeTestSubject();

		[NotNull]
		private FileSystem _testSubject;
		[NotNull]
		private FsDirectory _runRootFolder;

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
	}

	[TestFixture]
	public class CanIdentifyBasicPropertiesRealFs : CanIdentifyBasicProperties
	{
		protected override FileSystem MakeTestSubject()
		{
			return FileSystem.Real();
		}
	}

	[TestFixture]
	public class CanIdentifyBasicPropertiesMemoryFs : CanIdentifyBasicProperties
	{
		protected override FileSystem MakeTestSubject()
		{
			return FileSystem.Simulated();
		}
	}
}
