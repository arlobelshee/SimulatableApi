// SimulatableAPI
// File: CanComputePathsUsingFsPath.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System;
using System.IO;
using FluentAssertions;
using JetBrains.Annotations;
using NUnit.Framework;

namespace Simulated.Tests.FileSystemNavigation
{
	[TestFixture]
	public class CanComputePathsUsingFsPath
	{
		[Test]
		public void PathsShouldBeTheSameWhetherOrNotTheyHaveATrailingSlash()
		{
			(FsPath.TempFolder/@"thing\").Should()
				.Be(FsPath.TempFolder/"thing");
		}

		[Test]
		public void ShouldBeAbleToAppendRelativeFolderToRoot()
		{
			(FsPath.TempFolder/"folder")._Absolute.Should()
				.Be(FsPath.TempFolder._Absolute + Path.DirectorySeparatorChar + "folder");
		}

		[Test]
		public void ShouldBeAbleToAppendRelativeFolderToExistingPath()
		{
			(FsPath.TempFolder/"folder"/"sub")._Absolute.Should()
				.Be(FsPath.TempFolder._Absolute + Path.DirectorySeparatorChar + "folder" + Path.DirectorySeparatorChar + "sub");
		}

		[Test]
		public void AnyRootShouldBeKnownAsRoot()
		{
			FsPath.TempFolder.IsRoot.Should()
				.BeTrue();
		}

		[Test]
		public void NonRootPathsShouldBeKnownAsNotRoots()
		{
			(FsPath.TempFolder/"something.txt").IsRoot.Should()
				.BeFalse();
		}

		[Test]
		public void TempFolderShouldBeSystemTempFolder()
		{
			FsPath.TempFolder._Absolute.Should()
				.Be(Path.GetTempPath()
					.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
		}

		[Test]
		public void ShouldBeAbleToFindParentFolder()
		{
			(FsPath.TempFolder/"something.txt").Parent.Should()
				.Be(FsPath.TempFolder);
		}

		[Test]
		public void ShouldRejectAskingForParentOfRoot()
		{
			_Throws<BadStorageRequest>(() => { var foo = FsPath.TempFolder.Parent; }, @"'{Temp folder}' is a root. It does not have a parent.");
		}

		[Test]
		[TestCase(@"foo", @"foo\bar", true)]
		[TestCase(@"foo", @"foo\bar", false)]
		[TestCase(@"foo", @"foo\bar.txt", false)]
		[TestCase(@"foo", @"foo", true)]
		[TestCase(@"foo", @"foo\bar\baz", true)]
		[TestCase(@"foo", @"foo\bar\baz.txt", false)]
		[TestCase(@"", @"foo\bar\baz.txt", false)]
		public void ShouldBeAncestors([NotNull] string ancestor, [NotNull] string descendent, bool descendentIsDirectory)
		{
			(FsPath.TempFolder/ancestor).IsAncestorOf(FsPath.TempFolder/descendent, descendentIsDirectory)
				.Should()
				.BeTrue();
		}

		[Test]
		[TestCase(@"foo\bar", @"foo\bar.txt", false)]
		[TestCase(@"foo\foo", @"foo\bar", true)]
		[TestCase(@"bar\foo", @"bar", true)]
		[TestCase(@"foo", @"foo", false)]
		public void ShouldNotBeAncestors([NotNull] string ancestor, [NotNull] string nonDescendent, bool descendentIsDirectory)
		{
			(FsPath.TempFolder/ancestor).IsAncestorOf(FsPath.TempFolder/nonDescendent, descendentIsDirectory)
				.Should()
				.BeFalse();
		}

		[Test]
		[TestCase(@"a\b", @"e", true)]
		[TestCase(@"a\b\c", @"e\c", true)]
		[TestCase(@"a\b\c.txt", @"e\c.txt", false)]
		public void ReplacingValidAncestorsShouldSubstituteTheCommonPathElements([NotNull] string original, [NotNull] string expected, bool descendentIsDirectory)
		{
			(FsPath.TempFolder/original)._ReplaceAncestor(FsPath.TempFolder/@"a\b", FsPath.TempFolder/"e", descendentIsDirectory)
				.Should()
				.Be(FsPath.TempFolder/expected);
		}

		private static void _Throws<TException>([NotNull] Action code, [NotNull] string message) where TException : Exception
		{
			code.ShouldThrow<TException>()
				.WithMessage(message);
		}
	}
}
