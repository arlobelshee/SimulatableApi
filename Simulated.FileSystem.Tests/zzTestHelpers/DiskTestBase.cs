// SimulatableAPI
// File: DiskTestBase.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System;
using JetBrains.Annotations;
using NUnit.Framework;
using Simulated._Fs;

namespace Simulated.Tests.zzTestHelpers
{
	public abstract class DiskTestBase
	{
		[NotNull] protected const string ArbitraryFileContents = "contents";
		[NotNull] protected const string UnicodeContents = "helȽo ﺷ";
		[NotNull] protected FsPath BaseFolder;
		[NotNull] internal _IFsDisk TestSubject;

		[SetUp]
		public void Setup()
		{
			TestSubject = _MakeTestSubject();
			var runName = "TestRun-" + Guid.NewGuid()
				.ToString("N");
			BaseFolder = FsPath.TempFolder/runName;
			TestSubject.CreateDir(BaseFolder);
			FinishSetup();
		}

		[TearDown]
		public void Teardown()
		{
			BeginTeardown();
			TestSubject.DeleteDir(BaseFolder);
		}

		protected virtual void BeginTeardown() {}

		protected virtual void FinishSetup() {}

		[NotNull]
		internal abstract _IFsDisk _MakeTestSubject();
	}
}
