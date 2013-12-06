// SimulatableAPI
// File: _TestHelperExtensions.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using FluentAssertions;
using JetBrains.Annotations;
using Simulated._Fs;

namespace Simulated.Tests.zzTestHelpers
{
	internal static class _TestHelperExtensions
	{
		public static void ShouldExist(this FsDirectory dir)
		{
			dir.Exists.Result.Should()
				.BeTrue("directory should exist");
		}

		public static void ShouldContain([NotNull] this FsFile file, string contents)
		{
			file.Exists.Should()
				.BeTrue("file should exist");
			file.ReadAllText()
				.Should()
				.Be(contents);
		}

		public static void ShouldNotExist(this FsDirectory dir)
		{
			dir.Exists.Result.Should()
				.BeFalse("directory should be missing");
		}

		public static void ShouldNotExist(this FsFile file)
		{
			file.Exists.Should()
				.BeFalse();
		}
	}
}
