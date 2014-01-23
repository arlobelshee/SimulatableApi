// SimulatableAPI
// File: CanReadAndWriteFileContents.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using JetBrains.Annotations;
using NUnit.Framework;
using Simulated.Tests.zzTestHelpers;
using Simulated._Fs;

namespace Simulated.Tests.FileSystemModification
{
	public class CanReadAndWriteFileContents : FileSystemTestBase
	{
		[Test]
		public void AllFileObjectsWithTheSamePathShouldReferToSameStorage()
		{
			_testFile.Overwrite(OriginalContents);
			_testSubject.File(_testFile.FullPath)
				.Overwrite(NewContents);
			_testFile.ShouldContain(NewContents);
		}

		private const string OriginalContents = "Original contents";
		private const string NewContents = "helȽo ﺷ";
		[NotNull] private FsFile _testFile;

		protected override void FinishSetup()
		{
			_testFile = _runRootFolder.File("CreatedByTest.txt");
		}

		protected override FileSystem MakeTestSubject()
		{
			return FileSystem.Simulated();
		}
	}
}
