using System;
using JetBrains.Annotations;
using NUnit.Framework;
using Simulated._Fs;

namespace Simulated.Tests.zzTestHelpers
{
	public abstract class FileSystemTestBase
	{
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
			_runRootFolder = _testSubject.TempDirectory.Dir("CreatedByTestRun-" + Guid.NewGuid());
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
