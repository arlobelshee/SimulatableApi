// SimulatableAPI
// File: PublicApisCheckForValidInput.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System;
using System.Threading.Tasks;
using FluentAssertions;
using JetBrains.Annotations;
using NUnit.Framework;
using Simulated.Tests.zzTestHelpers;

namespace Simulated.Tests.QueryBasicProperties
{
	[TestFixture]
	public class PublicApisCheckForValidInput : FileSystemTestBase
	{
		[Test]
		public void FindingSubdirectoryRequiresValidPath()
		{
			_testSubject.Invoking(ts => ts.Directory((FsPath) null))
				.ShouldRejectNullForParam("path");
		}

		[Test]
		public void FindingSubdirectoryRequiresValidPathForStringsToo()
		{
			_testSubject.Invoking(ts => ts.Directory((string) null))
				.ShouldRejectNullForParam("absolutePath");
			_testSubject.Invoking(ts => ts.Directory(string.Empty))
				.ShouldRejectNullForParam("absolutePath");
		}

		[NotNull]
		[Test]
		public async Task FileReferencesFromDirectoryRequireValidName()
		{
			(await _testSubject.TempDirectory).Invoking(dir => dir.File(null))
				.ShouldRejectNullForParam("fileName");
		}

		[Test]
		public void DirectFileReferencesRequieValidName()
		{
			_testSubject.Invoking(ts => ts.File((FsPath) null))
				.ShouldRejectNullForParam("path");
		}

		[Test]
		public void DirectFileReferencesRequieValidNameForStringsToo()
		{
			_testSubject.Invoking(ts => ts.File((string) null))
				.ShouldRejectNullForParam("absolutePath");
			_testSubject.Invoking(ts => ts.File(string.Empty))
				.ShouldRejectNullForParam("absolutePath");
		}

		[Test]
		public void FsPathRequiresValidPath()
		{
			Action invalidConstruction = () => new FsPath(null);
			invalidConstruction.ShouldRejectNullForParam("absolutePath");
			invalidConstruction = () => new FsPath(string.Empty);
			invalidConstruction.ShouldRejectNullForParam("absolutePath");
		}

		[Test]
		public void FsPathRequiresAbsolutePath()
		{
			Action invalidConstruction = () => new FsPath(@"relative\path");
			invalidConstruction.ShouldThrow<ArgumentException>()
				.WithMessage(@"The path must be absolute. 'relative\path' is not an absolute path.*")
				.And.ParamName.Should()
				.Be("absolutePath");
		}

		protected override FileSystem MakeTestSubject()
		{
			return FileSystem.Simulated();
		}
	}
}
