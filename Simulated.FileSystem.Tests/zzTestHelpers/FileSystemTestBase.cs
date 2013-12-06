// SimulatableAPI
// File: FileSystemTestBase.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NUnit.Framework;
using Simulated._Fs;

namespace Simulated.Tests.zzTestHelpers
{
	public abstract class FileSystemTestBase
	{
		protected const string ArbitraryFileName = "something.txt";
		protected const string ArbitraryContents = "Arbitrary contents.";
		protected const string ArbitraryDirName = "ArbitraryDirectory";
		[NotNull] protected FileSystem _testSubject;
		[NotNull] protected FsDirectory _runRootFolder;

		protected virtual void FinishSetup() {}
		protected virtual void BeginTeardown() {}

		[NotNull]
		protected abstract FileSystem MakeTestSubject();

		[SetUp]
		public void Setup()
		{
			_testSubject = MakeTestSubject();
			_testSubject.EnableRevertToHere();
			_runRootFolder = _testSubject.TempDirectory.Result.Dir("CreatedByTestRun-" + Guid.NewGuid());
			FinishSetup();
		}

		[TearDown]
		public void Teardown()
		{
			BeginTeardown();
			_testSubject.RevertAllChanges();
		}
	}
}
