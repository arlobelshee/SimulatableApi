// SimulatableAPI
// File: FileSystemTestBase.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System;
using JetBrains.Annotations;
using NUnit.Framework;
using Simulated._Fs;

namespace Simulated.Tests.zzTestHelpers
{
	public abstract class FileSystemTestBase
	{
		protected const string ArbitraryFileName = "something.txt";
		[NotNull] protected FileSystem TestSubject;
		[NotNull] protected FsDirectory BaseFolder;

		protected virtual void FinishSetup() {}
		protected virtual void BeginTeardown() {}

		[SetUp]
		public void Setup()
		{
			TestSubject = FileSystem.Simulated();
			BaseFolder = TestSubject.TempDirectory.Dir("CreatedByTestRun-" + Guid.NewGuid());
			FinishSetup();
		}

		[TearDown]
		public void Teardown()
		{
			BeginTeardown();
			BaseFolder.EnsureDoesNotExist();
		}
	}
}
