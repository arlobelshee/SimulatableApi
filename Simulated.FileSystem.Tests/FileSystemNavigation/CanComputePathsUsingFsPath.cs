using System;
using System.IO;
using FluentAssertions;
using NUnit.Framework;

namespace Simulated.Tests.FileSystemNavigation
{
	[TestFixture]
	public class CanComputePathsUsingFsPath
	{
		[Test]
		public void ShouldRequirePathToBeNonEmpty()
		{
			_Throws<ArgumentNullException>(() => new FsPath(string.Empty),
				"A path cannot be null or empty.\r\nParameter name: absolutePath");
		}

		[Test]
		public void ShouldRequirePathToBeAbsolute()
		{
			_Throws<ArgumentException>(() => new FsPath(@"relative\path"),
				"The path must be absolute. 'relative\\path' is not an absolute path.\r\nParameter name: absolutePath");
		}

		[Test]
		public void PathsShouldBeTheSameWhetherOrNotTheyHaveATrailingSlash()
		{
			new FsPath(@"C:\Path\").Should().Be(new FsPath(@"C:\Path"));
		}

		[Test]
		public void PathsToDriveRootsShouldAlwaysEndInASlash()
		{
			new FsPath(@"C:\").Absolute.Should().Be(@"C:\");
		}

		[Test]
		public void ShouldBeAbleToAppendRelativeFolderToDriveRoot()
		{
			(new FsPath(@"C:\") / "folder").Should().Be(new FsPath(@"C:\folder"));
		}

		[Test]
		public void ShouldBeAbleToAppendRelativeFolderToExistingPath()
		{
			(new FsPath(@"C:\something") / "folder").Should().Be(new FsPath(@"C:\something\folder"));
		}

		[Test]
		public void DriveRootsShouldBeKnownAsRoots()
		{
			new FsPath(@"C:\").IsRoot.Should().BeTrue();
		}

		[Test]
		public void NonRootPathsShouldBeKnownAsNotRoots()
		{
			new FsPath(@"C:\something.txt").IsRoot.Should().BeFalse();
		}

		[Test]
		public void TempFolderShouldBeSystemTempFolder()
		{
			FsPath.TempFolder.Should().Be(new FsPath(Path.GetTempPath()));
		}

		[Test]
		public void ShouldBeAbleToFindParentFolder()
		{
			new FsPath(@"C:\something.txt").Parent.Should().Be(new FsPath(@"C:\"));
		}

		[Test]
		public void ShouldRejectAskingForParentOfRoot()
		{
			_Throws<InvalidOperationException>(() => { var foo = new FsPath(@"C:\").Parent; },
				@"'C:\' is a drive root. It does not have a parent.");
		}

		private static void _Throws<TException>(Action code, string message) where TException : Exception
		{
			code.ShouldThrow<TException>().WithMessage(message);
		}
	}
}
