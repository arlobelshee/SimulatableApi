// SimulatableAPI
// File: DiskTestBase.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System;
using JetBrains.Annotations;
using NUnit.Framework;
using Simulated._Fs;

namespace Simulated.Tests.EncapsulateDifferencesBetweenDiskAndMemory
{
	public abstract class DiskTestBase
	{
		protected const string ArbitraryFileContents = "contents";
		protected const string UnicodeContents = "helȽo ﺷ";
		protected FsPath BaseFolder;
		internal _IFsDisk TestSubject;

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
