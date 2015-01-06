// SimulatableAPI
// File: FsDirectory.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Simulated._Fs
{
	/// <summary>
	///    Represents a folder in the underlying data store. This folder may or may not exist. This class exposes methods to
	///    create and delete folders, to manipulate their contents, and to ask for more information about the folder.
	/// </summary>
	[PublicApi]
	public class FsDirectory : IEquatable<FsDirectory>
	{
		[NotNull] private readonly FileSystem _allFiles;
		[NotNull] private readonly FsPath _path;

		internal FsDirectory([NotNull] FileSystem allFiles, [NotNull] FsPath path)
		{
			if (path == null)
				throw new ArgumentNullException("path");
			if (allFiles == null)
				throw new ArgumentNullException("allFiles");
			_path = path;
			_allFiles = allFiles;
		}

		/// <summary>
		///    Gets a value indicating whether this <see cref="FsDirectory" /> exists.
		/// </summary>
		/// <value> <c>true</c> if it exists; otherwise, <c>false</c> . </value>
		[PublicApi]
		public bool Exists
		{
			get { return _allFiles._Disk.DirExistsNeedsToBeMadeDelayStart(_path); }
		}

		/// <summary>
		///    Gets the path to this directory.
		/// </summary>
		[NotNull]
		[PublicApi]
		public FsPath Path
		{
			get { return _path; }
		}

		/// <summary>
		///    Gets the directory that contains this directory.
		/// </summary>
		[NotNull]
		[PublicApi]
		public FsDirectory Parent
		{
			get { return new FsDirectory(_allFiles, _path.Parent); }
		}

		/// <summary>
		///    Gets a directory instance that represents a sub-directory of this directory.
		/// </summary>
		/// <param name="subdirName"> Name of the subdir. </param>
		/// <returns> the subdir as a directory object </returns>
		[NotNull]
		[PublicApi]
		public FsDirectory Dir([NotNull] string subdirName)
		{
			return new FsDirectory(_allFiles, _path/subdirName);
		}

		/// <summary>
		///    Gets a directory instance that represents a sub-directory of this directory.
		///    Identical to <see cref="Dir" />.
		/// </summary>
		/// <param name="self">The absolute path.</param>
		/// <param name="subdirName"> Name of the subdir. </param>
		/// <returns> the subdir as a directory object </returns>
		[PublicApi]
		[NotNull]
		public static FsDirectory operator /([NotNull] FsDirectory self, [NotNull] string subdirName)
		{
			return self.Dir(subdirName);
		}

		/// <summary>
		///    Gets a file object for a file in this directory.
		/// </summary>
		/// <param name="fileName">Name of the file.</param>
		/// <returns>a file in this directory</returns>
		[NotNull]
		[PublicApi]
		public FsFile File([NotNull] string fileName)
		{
			return new FsFile(_allFiles, _path/fileName);
		}

		/// <summary>
		///    Regardless of the previous state of the file system, results in a directory existing at this object's Path. This
		///    operation is revertable.
		/// </summary>
		[NotNull]
		[PublicApi]
		public Task EnsureExists()
		{
			return _allFiles._Disk.CreateDir(_path);
		}

		/// <summary>
		///    Regardless of the previous state of the file system, results in a directory no longer existing at this object's
		///    Path. This operation is revertable.
		/// </summary>
		[PublicApi]
		public void EnsureDoesNotExist()
		{
			_allFiles._Disk.DeleteDirNeedsToBeMadeDelayStart(_path);
		}

		/// <summary>
		///    Gets an enumeration of known files in this directory.
		///    Note: an in-memory FileSystem is always presumed to start empty, while an on-disk
		///    FileSystem starts with an existing structure of directories and files.
		///    This method always returns results from the underlying data store. As such, it will find files that you created
		///    with prior to this FileSystem's existence (e.g., those created in a root from which this was
		///    cloned or those that were on the disk before attaching this FileSystem).
		/// </summary>
		/// <param name="searchPattern">A filter to apply. Uses file system shell pattern matching (e.g., *.txt).</param>
		/// <returns>An enumeration of all known files that match the pattern.</returns>
		[NotNull]
		[PublicApi]
		public IEnumerable<FsFile> Files([NotNull] string searchPattern)
		{
			return _allFiles._Disk.FindFilesNeedsToBeMadeDelayStart(_path, searchPattern)
				.Select(p => new FsFile(_allFiles, p));
		}

		/// <summary>
		///    Indicates whether two folders represent the same path. They may come from different file systems and still be termed
		///    equal.
		/// </summary>
		/// <param name="other"> A directory instance to compare with this object. </param>
		/// <returns> true if the two objects have the same path; otherwise, false. </returns>
		[PublicApi]
		public bool Equals([CanBeNull] FsDirectory other)
		{
			if (ReferenceEquals(null, other))
				return false;
			if (ReferenceEquals(this, other))
				return true;
			return Equals(other._path, _path);
		}

		/// <summary>
		///    Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		/// <returns> A <see cref="System.String" /> that represents this instance. </returns>
		[PublicApi]
		public override string ToString()
		{
			return string.Format("Directory({0})", _path);
		}

		/// <summary>
		///    Indicates whether two folders represent the same path. They may come from different file systems and still be termed
		///    equal.
		/// </summary>
		/// <param name="obj"> A directory instance to compare with this object. </param>
		/// <returns> true if the two objects have the same path; otherwise, false. </returns>
		[PublicApi]
		public override bool Equals([NotNull] object obj)
		{
			return Equals(obj as FsDirectory);
		}

		/// <summary>
		///    Returns a hash code for this instance.
		/// </summary>
		/// <returns> A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. </returns>
		[PublicApi]
		public override int GetHashCode()
		{
			return _path.GetHashCode();
		}

		/// <summary>
		///    Implements the operator ==. It is the same as Equals.
		/// </summary>
		/// <param name="left"> The left. </param>
		/// <param name="right"> The right. </param>
		/// <returns> The result of the operator. </returns>
		[PublicApi]
		public static bool operator ==(FsDirectory left, FsDirectory right)
		{
			return Equals(left, right);
		}

		/// <summary>
		///    Implements the operator !=. It is the same as !(left == right)
		/// </summary>
		/// <param name="left"> The left. </param>
		/// <param name="right"> The right. </param>
		/// <returns> The result of the operator. </returns>
		[PublicApi]
		public static bool operator !=(FsDirectory left, FsDirectory right)
		{
			return !Equals(left, right);
		}
	}
}
