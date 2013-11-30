﻿using System;
using FluentAssertions;
using JetBrains.Annotations;
using NUnit.Framework;
using Simulated.Tests.zzTestHelpers;

namespace Simulated.Tests.FileSystemNavigation
{
	public abstract class CanFindEntryPoints : FileSystemTestBase
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
			_testSubject.Directory(ArbitraryMissingFolder).Path.Absolute.Should().Be(ArbitraryMissingFolder);
		}


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
