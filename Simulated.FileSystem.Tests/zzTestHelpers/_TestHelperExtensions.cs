// SimulatableAPI
// File: _TestHelperExtensions.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System;
using FluentAssertions;
using JetBrains.Annotations;
using Simulated._Fs;

namespace Simulated.Tests.zzTestHelpers
{
	internal static class _TestHelperExtensions
	{
		public static void ShouldExist([NotNull] this FsDirectory dir)
		{
			dir.Exists.Result.Should()
				.BeTrue("directory {0} should exist", dir.Path.Absolute);
		}

		public static void ShouldExist([NotNull] this FsFile file)
		{
			file.Exists.Result.Should()
				.BeTrue("file {0} should exist", file.FullPath.Absolute);
		}

		public static void ShouldContain([NotNull] this FsFile file, [NotNull] string contents)
		{
			file.ShouldExist();
			file.ReadAllText()
				.Result.Should()
				.Be(contents);
		}

		public static void ShouldNotExist([NotNull] this FsDirectory dir)
		{
			dir.Exists.Result.Should()
				.BeFalse("directory {0} should be missing", dir.Path.Absolute);
		}

		public static void ShouldNotExist([NotNull] this FsFile file)
		{
			file.Exists.Result.Should()
				.BeFalse("file {0} should be missing", file.FullPath.Absolute);
		}

		public static void ShouldRejectNullForParam([NotNull] this Action invoking, [NotNull] string paramName)
		{
			invoking.ShouldThrow<ArgumentNullException>()
				.And.ParamName.Should()
				.Be(paramName);
		}
	}
}
