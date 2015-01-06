// SimulatableAPI
// File: FileSystem.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Simulated._Fs;

namespace Simulated
{
	/// <summary>
	///    Represents a view on the file system. The underlying store could be a real file system or a simulated (in-memory)
	///    one. In either case, FileSystem and its helpers allow a user to interact with an abstraction of this storage.
	///    All oprations are performed asynchronously. The library overlapps operations that do not
	///    impact each other. No guarantees are made in case of an application crash.
	///    Each FileSystem instance is its own view of the storage.
	/// </summary>
	[PublicApi]
	public class FileSystem
	{
		private FileSystem([NotNull] _IFsDisk disk)
		{
			_Disk = disk;
			TempDirectory.EnsureExists().RunSynchronouslyAsCheapHackUntilIFixScheduling();
		}

		[NotNull]
		internal _IFsDisk _Disk { get; private set; }

		/// <summary>
		///    Gets the temp directory.
		/// </summary>
		[NotNull]
		[PublicApi]
		public FsDirectory TempDirectory
		{
			get { return Directory(FsPath.TempFolder); }
		}

		/// <summary>
		///    Creates a file system instance backed by the actual file system.
		/// </summary>
		/// <returns>an on-disk file system</returns>
		[NotNull]
		[PublicApi]
		public static FileSystem Real()
		{
			return new FileSystem(new _DiskReal());
		}

		/// <summary>
		///    Creates a file system instance that is backed by in-memory storage.
		/// </summary>
		/// <returns>an in-memory file system</returns>
		[NotNull]
		[PublicApi]
		public static FileSystem Simulated()
		{
			return new FileSystem(new _DiskSimulated());
		}

		/// <summary>
		///    Creates a directory instance for a path on this file system.
		///    The directory need not exist in the file system storage. Creating a directory object
		///    will not create the directory.
		/// </summary>
		/// <param name="path">The path this object should represent.</param>
		/// <exception cref="ArgumentNullException">if the path is null</exception>
		/// <returns>a non-null directory instance</returns>
		[NotNull]
		[PublicApi]
		public FsDirectory Directory([NotNull] FsPath path)
		{
			return new FsDirectory(this, path);
		}

		/// <summary>
		///    Creates a file instance for a path on this file system.
		///    The file need not exist in the file system storage. Creating a file object
		///    will not create the file.
		/// </summary>
		/// <param name="fileName">The path this object should represent.</param>
		/// <exception cref="ArgumentNullException">if the path is null</exception>
		/// <returns>a non-null file instance</returns>
		[NotNull]
		[PublicApi]
		public FsFile File([NotNull] FsPath fileName)
		{
			return new FsFile(this, fileName);
		}
	}
}
