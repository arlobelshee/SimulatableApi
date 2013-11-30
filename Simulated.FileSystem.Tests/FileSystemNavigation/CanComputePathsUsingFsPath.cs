// SimulatableAPI
// File: CanComputePathsUsingFsPath.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

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
			_Throws<ArgumentNullException>(() => new FsPath(string.Empty), "A path cannot be null or empty.\r\nParameter name: absolutePath");
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
			new FsPath(@"C:\Path\").Should()
				.Be(new FsPath(@"C:\Path"));
		}

		[Test]
		public void PathsToDriveRootsShouldAlwaysEndInASlash()
		{
			new FsPath(@"C:\").Absolute.Should()
				.Be(@"C:\");
		}

		[Test]
		public void ShouldBeAbleToAppendRelativeFolderToDriveRoot()
		{
			(new FsPath(@"C:\")/"folder").Should()
				.Be(new FsPath(@"C:\folder"));
		}

		[Test]
		public void ShouldBeAbleToAppendRelativeFolderToExistingPath()
		{
			(new FsPath(@"C:\something")/"folder").Should()
				.Be(new FsPath(@"C:\something\folder"));
		}

		[Test]
		public void DriveRootsShouldBeKnownAsRoots()
		{
			new FsPath(@"C:\").IsRoot.Should()
				.BeTrue();
		}

		[Test]
		public void NonRootPathsShouldBeKnownAsNotRoots()
		{
			new FsPath(@"C:\something.txt").IsRoot.Should()
				.BeFalse();
		}

		[Test]
		public void TempFolderShouldBeSystemTempFolder()
		{
			FsPath.TempFolder.Should()
				.Be(new FsPath(Path.GetTempPath()));
		}

		[Test]
		public void ShouldBeAbleToFindParentFolder()
		{
			new FsPath(@"C:\something.txt").Parent.Should()
				.Be(new FsPath(@"C:\"));
		}

		[Test]
		public void ShouldRejectAskingForParentOfRoot()
		{
			_Throws<InvalidOperationException>(() => { var foo = new FsPath(@"C:\").Parent; }, @"'C:\' is a drive root. It does not have a parent.");
		}

		[Test]
		[TestCase(@"C:\foo", @"C:\foo\bar", true)]
		[TestCase(@"C:\foo", @"C:\foo\bar", false)]
		[TestCase(@"C:\foo", @"C:\foo\bar.txt", false)]
		[TestCase(@"C:\foo", @"C:\foo", true)]
		[TestCase(@"C:\foo", @"C:\foo\bar\baz", true)]
		[TestCase(@"C:\foo", @"C:\foo\bar\baz.txt", false)]
		[TestCase(@"C:\", @"C:\foo\bar\baz.txt", false)]
		public void ShouldBeAncestors(string ancestor, string descendent, bool descendentIsDirectory)
		{
			new FsPath(ancestor).IsAncestorOf(new FsPath(descendent), descendentIsDirectory)
				.Should()
				.BeTrue();
		}

		[Test]
		[TestCase(@"D:\foo", @"C:\foo\bar", true)]
		[TestCase(@"D:\foo", @"C:\foo\bar.txt", false)]
		[TestCase(@"D:\foo", @"C:\foo", true)]
		[TestCase(@"C:\foo\bar", @"C:\foo\bar.txt", false)]
		[TestCase(@"D:\", @"C:\foo\bar\baz.txt", false)]
		[TestCase(@"C:\foo\foo", @"C:\foo\bar", true)]
		[TestCase(@"C:\bar\foo", @"C:\bar", true)]
		[TestCase(@"C:\foo", @"C:\foo", false)]
		public void ShouldNotBeAncestors(string ancestor, string nonDescendent, bool descendentIsDirectory)
		{
			new FsPath(ancestor).IsAncestorOf(new FsPath(nonDescendent), descendentIsDirectory)
				.Should()
				.BeFalse();
		}

		[Test]
		[TestCase(@"C:\a\b", @"C:\e", true)]
		[TestCase(@"C:\a\b\c", @"C:\e\c", true)]
		[TestCase(@"C:\a\b\c.txt", @"C:\e\c.txt", false)]
		public void ReplacingValidAncestorsShouldSubstituteTheCommonPathElements(string original, string expected, bool descendentIsDirectory)
		{
			new FsPath(original).ReplaceAncestor(new FsPath(@"C:\a\b"), new FsPath(@"C:\e"), descendentIsDirectory)
				.Should()
				.Be(new FsPath(expected));
		}

		private static void _Throws<TException>(Action code, string message) where TException : Exception
		{
			code.ShouldThrow<TException>()
				.WithMessage(message);
		}
	}
}
