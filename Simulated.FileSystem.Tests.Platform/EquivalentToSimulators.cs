// SimulatableAPI
// File: EquivalentToSimulators.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using NUnit.Framework;
using Simulated.Tests.FileSystemModification;
using Simulated.Tests.FileSystemNavigation;
using Simulated.Tests.QueryBasicProperties;

namespace Simulated.Tests.Platform
{
	[TestFixture]
	public class CanDeleteThingsInRealFs : CanDeleteThings
	{
		protected override FileSystem MakeTestSubject()
		{
			return FileSystem.Real();
		}
	}

	[TestFixture]
	public class CanNavigateRelativeToAFileOrDirectoryRealFs : CanNavigateRelativeToAFileOrDirectory
	{
		protected override FileSystem MakeTestSubject()
		{
			return FileSystem.Real();
		}
	}

	[TestFixture]
	public class CanCreateDirectoriesInRealFs : CanCreateDirectories
	{
		protected override FileSystem MakeTestSubject()
		{
			return FileSystem.Real();
		}
	}

	[TestFixture]
	public class CanReadAndWriteFileContentsInRealFs : CanReadAndWriteFileContents
	{
		protected override FileSystem MakeTestSubject()
		{
			return FileSystem.Real();
		}
	}

	[TestFixture]
	public class CanFindEntryPointsRealFs : CanFindEntryPoints
	{
		protected override FileSystem MakeTestSubject()
		{
			return FileSystem.Real();
		}
	}

	[TestFixture]
	public class CanIdentifyBasicPropertiesRealFs : CanIdentifyBasicProperties
	{
		protected override FileSystem MakeTestSubject()
		{
			return FileSystem.Real();
		}
	}
}
