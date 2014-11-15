// SimulatableAPI
// File: ProvideLowLevelOperationsThatWillDelayIfNecessary.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System.Threading.Tasks;
using FluentAssertions;
using JetBrains.Annotations;
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
		[Category(_Categories.Integration)]
		public void DelayingDiskShouldEnsureEachOperationExecutesOnlyWhenSafeToDoSo()
		{
			var storage = new _BlockingDiskInMemory();
			var testSubject = new _DelayingDisk(storage);
			var arbitraryPath = FsPath.TempFolder/"A";

			testSubject.Overwrite(arbitraryPath, ArbitraryFileContents);
			var existenceCheck = testSubject.FileExists(arbitraryPath);

			existenceCheck.ShouldNotHaveRun();
			storage.Impl.ShouldNotExist(arbitraryPath);

			storage.ExecuteAllPendingActionsSynchronously();

			existenceCheck.ShouldNotHaveRun();
			storage.Impl.ShouldBeFile(arbitraryPath, ArbitraryFileContents);

			storage.ExecuteAllPendingActionsSynchronously();

			existenceCheck.ShouldHaveRun();
			existenceCheck.Result.Should()
				.BeTrue();
		}
	}
}
