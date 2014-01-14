// SimulatableAPI
// File: FsDirectory.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Simulated._Fs
{
	/// <summary>
	///    Represents a folder in the underlying data store. This folder may or may not exist. This class exposes methods to
	///    create and delete folders, to manipulate their contents, and to ask for more information about the folder.
	/// </summary>
	public class FsDirectory : IEquatable<FsDirectory>
	{
		[NotNull] private readonly FileSystem _allFiles;
		[NotNull] private readonly FsPath _path;
		[NotNull] private readonly _Storage _underlyingStorage;

		internal FsDirectory([NotNull] FileSystem allFiles, [NotNull] FsPath path, [NotNull] _Storage underlyingStorage)
		{
			if (path == null)
				throw new ArgumentNullException("path");
			if (allFiles == null)
				throw new ArgumentNullException("allFiles");
			if (underlyingStorage == null)
				throw new ArgumentNullException("underlyingStorage");
			_path = path;
			_allFiles = allFiles;
			_underlyingStorage = underlyingStorage;
		}

		/// <summary>
		///    Gets a value indicating whether this <see cref="FsDirectory" /> exists.
		/// </summary>
		/// <value>&lt;c&gt;true&lt;/c&gt; if it exists; otherwise, &lt;c&gt;false&lt;/c&gt;.</value>
		[NotNull]
		public Task<bool> Exists
		{
			get { return _underlyingStorage.IsDirectory(_path); }
		}

		/// <summary>
		///    Gets the path to this directory.
		/// </summary>
		[NotNull]
		public FsPath Path
		{
			get { return _path; }
		}

		/// <summary>
		///    Gets the directory that contains this directory.
		/// </summary>
		[NotNull]
		public FsDirectory Parent
		{
			get { return new FsDirectory(_allFiles, _path.Parent, _underlyingStorage); }
		}

		/// <summary>
		///    Gets the FileSystem that holds this directory.
		/// </summary>
		[NotNull]
		public FileSystem FileSystem
		{
			get { return _allFiles; }
		}

		/// <summary>
		///    Gets a drectory instance that represents a sub-directory of this directory.
		/// </summary>
		/// <param name="subdirName"> Name of the subdir. </param>
		/// <returns> the subdir as a directory object </returns>
		[NotNull]
		public FsDirectory Dir([NotNull] string subdirName)
		{
			return new FsDirectory(_allFiles, _path/subdirName, _underlyingStorage);
		}

		/// <summary>
		///    Regardless of the previous state of the file system, results in a directory existing at this object's Path. This
		///    operation is revertable.
		/// </summary>
		[NotNull]
		public Task EnsureExists()
		{
			return _underlyingStorage.EnsureDirectoryExists(_path);
		}

		/// <summary>
		/// Ensures the directory exists in the background and gives back a lazy reference to it.
		/// </summary>
		/// <returns>An awaitable that will guarantee the directory exists when await returns.</returns>
		[NotNull]
		public AsyncLazy<FsDirectory> CreateInBackground()
		{
			return new AsyncLazy<FsDirectory>(EnsureExists()
				.ContinueWith(result =>
				{
					result.Wait(); // observe any exceptions.
					return this;
				}));
		}

		/// <summary>
		///    Regardless of the previous state of the file system, results in a directory no longer existing at this object's
		///    Path. This operation is revertable.
		/// </summary>
		[NotNull]
		public Task EnsureDoesNotExist()
		{
			return _underlyingStorage.EnsureDirectoryDoesNotExist(_path);
		}

		/// <summary>
		///    Gets a file object for a file in this directory.
		/// </summary>
		/// <param name="fileName">Name of the file.</param>
		/// <returns>a file in this directory</returns>
		[NotNull]
		public FsFile File([NotNull] string fileName)
		{
			return new FsFile(_allFiles, _path/fileName, _underlyingStorage);
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
		public async Task<IEnumerable<FsFile>> FilesThatExist([NotNull] string searchPattern)
		{
			return await _underlyingStorage.KnownFilesIn(searchPattern, _path);
		}

		/// <summary>
		///    Indicates whether two folders represent the same path. They may come from different file systems and still be termed
		///    equal.
		/// </summary>
		/// <param name="other"> A directory instance to compare with this object. </param>
		/// <returns> true if the two objects have the same path; otherwise, false. </returns>
		public bool Equals([CanBeNull] FsDirectory other)
		{
			if (ReferenceEquals(null, other))
				return false;
			if (ReferenceEquals(this, other))
				return true;
			return Equals(other._path, _path);
		}

		/// <summary>
		///    A programmer display of the type and value data for this directory.
		/// </summary>
		/// <returns> A <see cref="System.String" /> representation for help debugging. </returns>
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
		public override bool Equals([CanBeNull] object obj)
		{
			return Equals(obj as FsDirectory);
		}

		/// <summary>
		///    Returns a hash code for this instance.
		/// </summary>
		/// <returns> A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. </returns>
		public override int GetHashCode()
		{
			return _path.GetHashCode();
		}

		/// <summary>
		///    Implements the operator ==.
		/// </summary>
		/// <param name="left"> The left. </param>
		/// <param name="right"> The right. </param>
		/// <returns> The result of the operator. </returns>
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
		public static bool operator !=(FsDirectory left, FsDirectory right)
		{
			return !Equals(left, right);
		}
	}
}
