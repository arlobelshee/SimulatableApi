using NUnit.Framework;
using Simulated.Tests.zzTestHelpers;

namespace Simulated.Tests.FileSystemModification
{
	public abstract class CanDeleteThings : FileSystemTestBase
	{
		[Test]
		public void DeletingADirectoryThatDoesNotExistShouldDoNothing()
		{
			var dir = _runRootFolder.Dir(ArbitraryDirName);
			dir.ShouldNotExist();
			dir.EnsureDoesNotExist();
			dir.ShouldNotExist();
		}

		[Test]
		public void DeletingANewlyCreatedDirectoryShouldEliminateItAsIfItWereNeverThere()
		{
			var dir = _runRootFolder.Dir(ArbitraryDirName);
			dir.EnsureExists();
			dir.ShouldExist();
			dir.EnsureDoesNotExist();
			dir.ShouldNotExist();
		}

		[Test]
		public void RevertingADirectoryDeleteShouldRestoreSubdirsAndFiles()
		{
			var dir = _runRootFolder.Dir(ArbitraryDirName);
			dir.EnsureExists();
			dir.Dir(ArbitraryDirName).EnsureExists();
			dir.File(ArbitraryFileName).Overwrite(ArbitraryContents);
			using (var fs = _testSubject.Clone())
			{
				fs.EnableRevertToHere();
				fs.Directory(dir.Path).EnsureDoesNotExist();
				dir.ShouldNotExist();
				dir.Dir(ArbitraryDirName).ShouldNotExist();
				dir.File(ArbitraryFileName).ShouldNotExist();
			}
			dir.ShouldExist();
			dir.Dir(ArbitraryDirName).ShouldExist();
			dir.File(ArbitraryFileName).ShouldContain(ArbitraryContents);
		}
	}

	[TestFixture]
	public class CanDeleteThingsInRealFs : CanDeleteThings
	{
		protected override FileSystem MakeTestSubject()
		{
			return FileSystem.Real();
		}
	}

	[TestFixture]
	public class CanDeleteThingsInMemoryFs : CanDeleteThings
	{
		protected override FileSystem MakeTestSubject()
		{
			return FileSystem.Simulated();
		}
	}
}
