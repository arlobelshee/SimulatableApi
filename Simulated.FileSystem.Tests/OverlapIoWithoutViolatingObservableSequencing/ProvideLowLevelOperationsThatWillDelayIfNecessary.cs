// SimulatableAPI
// File: ProvideLowLevelOperationsThatWillDelayIfNecessary.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using FluentAssertions;
using NUnit.Framework;
using Simulated.Tests.zzTestHelpers;
using Simulated._Fs;

namespace Simulated.Tests.OverlapIoWithoutViolatingObservableSequencing
{
	[TestFixture]
	public class ProvideLowLevelOperationsThatWillDelayIfNecessary
	{
		private const string ArbitraryFileContents = "hello";

		[Test]
		[Ignore(@"Not yet ready. There is an internal problem with something not getting delayed properly.
I need to refactor first to make that problem easier to solve.
Current design makes this hard to do right.")]
		[Category(_Categories.Integration)]
		public void DelayingDiskShouldEnsureEachOperationExecutesOnlyWhenSafeToDoSo()
		{
			var storage = new _BlockingDiskInMemory();
			var testSubject = new _ConflictFreeDisk(storage);
			var arbitraryPath = FsPath.TempFolder/"A";

			testSubject.Overwrite(arbitraryPath, ArbitraryFileContents);
			var existenceCheck = testSubject.FileExists(arbitraryPath);

			existenceCheck.ShouldNotHaveRun();
			storage.Impl.ShouldNotExist(arbitraryPath);

			storage.ExecuteAllRequestedActionsSynchronously();

			existenceCheck.ShouldNotHaveRun();
			storage.Impl.ShouldBeFile(arbitraryPath, ArbitraryFileContents);

			storage.ExecuteAllRequestedActionsSynchronously();

			existenceCheck.ShouldHaveRun();
			existenceCheck.Result.Should()
				.BeTrue();
		}
	}
}
