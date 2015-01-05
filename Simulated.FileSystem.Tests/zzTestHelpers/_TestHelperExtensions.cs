// SimulatableAPI
// File: _TestHelperExtensions.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
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

		public static void ShouldExist([NotNull] this FsFile file)
		{
			file.Exists.Result.Should()
				.BeTrue();
		}

		public static void ShouldContain([NotNull] this FsFile file, [NotNull] string contents)
		{
			file.Exists.Result.Should()
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
			file.Exists.Result.Should()
				.BeFalse();
		}

		public static void ShouldBeDir([NotNull] this _IFsDisk disk, [NotNull] FsPath dir)
		{
			disk.DirExistsNeedsToBeMadeDelayStart(dir)
				.Should()
				.BeTrue();
		}

		public static void ShouldBeFile([NotNull] this _IFsDisk disk, [NotNull] FsPath file, [NotNull] string contents)
		{
			disk.FileExistsNeedsToBeMadeDelayStart(file)
				.Result.Should()
				.BeTrue();
			disk.TextContentsNeedsToBeMadeDelayStart(file)
				.Result.Should()
				.Be(contents);
		}

		public static void ShouldBeFile([NotNull] this _IFsDisk disk, [NotNull] FsPath file, [NotNull] byte[] contents)
		{
			disk.FileExistsNeedsToBeMadeDelayStart(file)
				.Result.Should()
				.BeTrue();
			disk.RawContentsNeedsToBeMadeDelayStart(file)
				.CollectAllBytes()
				.Should()
				.Equal(contents);
		}

		public static void ShouldNotExist([NotNull] this _IFsDisk disk, [NotNull] FsPath path)
		{
			disk.ShouldNotBeDir(path);
			disk.ShouldNotBeFile(path);
		}

		public static void ShouldNotBeFile([NotNull] this _IFsDisk disk, [NotNull] FsPath path)
		{
			disk.FileExistsNeedsToBeMadeDelayStart(path)
				.Result.Should()
				.BeFalse();
		}

		public static void ShouldNotBeDir([NotNull] this _IFsDisk disk, [NotNull] FsPath path)
		{
			disk.DirExistsNeedsToBeMadeDelayStart(path)
				.Should()
				.BeFalse();
		}

		[NotNull]
		public static IEnumerable<byte> CollectAllBytes([NotNull] this IObservable<byte[]> rawContents)
		{
			var actual = rawContents.SelectMany(b => b)
				.ToEnumerable();
			return actual;
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

		public static void ShouldHaveRun([NotNull] this Task<bool> existsResult)
		{
			existsResult.IsCompleted.Should()
				.BeTrue();
		}

		public static void ShouldNotHaveRun(this Task<bool> existsResult)
		{
			existsResult.IsCompleted.Should()
				.BeFalse();
		}
	}
}
