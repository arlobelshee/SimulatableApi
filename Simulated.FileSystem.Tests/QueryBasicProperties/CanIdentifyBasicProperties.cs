using FluentAssertions;
using NUnit.Framework;
using Simulated.Tests.zzTestHelpers;

namespace Simulated.Tests.QueryBasicProperties
{
	public abstract class CanIdentifyBasicProperties : FileSystemTestBase
	{
		[Test]
		public void AFileKnowsItsFileNameParts()
		{
			const string fileName = "ArbitraryFile.txt";
			const string baseName = "ArbitraryFile";
			const string extension = ".txt";

			var f = _runRootFolder.File(fileName);
			f.FileName.Should().Be(fileName);
			f.Extension.Should().Be(extension);
			f.FileBaseName.Should().Be(baseName);
		}

		[Test]
		public void DirectoriesKnowWhetherTheyExist()
		{
			_runRootFolder.Exists.Should().BeFalse();
			_runRootFolder.Parent.Exists.Should().BeTrue();
		}

		[Test]
		public void FilesKnowWhetherTheyExist()
		{
			var testSubject = _runRootFolder.File("something.txt");
			testSubject.Exists.Should().BeFalse();
			testSubject.Overwrite("anything");
			testSubject.Exists.Should().BeTrue();
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
