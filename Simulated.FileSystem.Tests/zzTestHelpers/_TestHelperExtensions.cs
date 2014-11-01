// SimulatableAPI
// File: _TestHelperExtensions.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using FluentAssertions;
using JetBrains.Annotations;
using Simulated._Fs;

namespace Simulated.Tests.zzTestHelpers
{
	internal static class _TestHelperExtensions
	{
		public static void ShouldExist([NotNull] this FsDirectory dir)
		{
			dir.Exists.Should()
				.BeTrue();
		}

		public static void ShouldContain([NotNull] this FsFile file, [NotNull] string contents)
		{
			file.Exists.Should()
				.BeTrue();
			file.ReadAllText()
				.Result.Should()
				.Be(contents);
		}

		public static void ShouldNotExist([NotNull] this FsDirectory dir)
		{
			dir.Exists.Should()
				.BeFalse();
		}

		public static void ShouldNotExist([NotNull] this FsFile file)
		{
			file.Exists.Should()
				.BeFalse();
		}

		public static void ShouldBeDir([NotNull] this _IFsDisk disk, [NotNull] FsPath dir)
		{
			disk.DirExists(dir)
				.Should()
				.BeTrue();
		}

		public static void ShouldBeFile([NotNull] this _IFsDisk disk, [NotNull] FsPath file, [NotNull] string contents)
		{
			disk.FileExists(file)
				.Should()
				.BeTrue();
			disk.TextContents(file)
				.Result.Should()
				.Be(contents);
		}

		public static void ShouldBeFile([NotNull] this _IFsDisk disk, [NotNull] FsPath file, [NotNull] byte[] contents)
		{
			disk.FileExists(file)
				.Should()
				.BeTrue();
			disk.RawContents(file)
				.CollectAllBytes()
				.Should()
				.Equal(contents);
		}

		[NotNull]
		public static IEnumerable<byte> CollectAllBytes([NotNull] this IObservable<byte[]> rawContents)
		{
			var actual = rawContents.SelectMany(b => b)
				.ToEnumerable();
			return actual;
		}

		public static void ShouldNotExist([NotNull] this _IFsDisk disk, [NotNull] FsPath path)
		{
			disk.DirExists(path)
				.Should()
				.BeFalse();
			disk.FileExists(path)
				.Should()
				.BeFalse();
		}

		[NotNull]
		public static List<_ParallelSafeWorkSet> ScheduledWork([NotNull] this _OperationBacklog testSubject)
		{
			return testSubject.ShouldRaise("WorkIsReadyToExecute")
				.Select(evt => evt.Parameters.Skip(1)
					.Cast<_ParallelSafeWorkSet>()
					.Single())
				.ToList();
		}
	}
}
