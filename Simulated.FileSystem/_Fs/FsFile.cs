// SimulatableAPI
// File: FsFile.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Simulated._Fs
{
	/// <summary>
	///    Represents a file in the underlying data store. This file may or may not exist. This class exposes methods to read,
	///    write, and delete the file. It also exposes methods to ask for information about the file. Multiple file instances
	///    can have the same path and
	///    <see
	///       cref="FileSystem" />
	///    . If so, they will share storage. Any change made in one will be immediately visible in the all others.
	/// </summary>
	[PublicApi]
	public class FsFile : IEquatable<FsFile>
	{
		[NotNull] private readonly FileSystem _allFiles;
		[NotNull] private readonly FsPath _path;

		internal FsFile([NotNull] FileSystem allFiles, [NotNull] FsPath path)
		{
			if (allFiles == null)
				throw new ArgumentNullException("allFiles");
			if (path == null)
				throw new ArgumentNullException("path");
			_allFiles = allFiles;
			_path = path;
		}

		/// <summary>
		///    Gets the folder that contains this file.
		/// </summary>
		[PublicApi]
		[NotNull]
		public FsDirectory ContainingFolder
		{
			get { return new FsDirectory(_allFiles, _path.Parent); }
		}

		/// <summary>
		///    Gets a value indicating whether this <see cref="FsFile" /> exists.
		/// </summary>
		/// <value> <c>true</c> if it exists; otherwise, <c>false</c> . </value>
		[NotNull]
		[PublicApi]
		public Task<bool> Exists
		{
			get { return _allFiles._Disk.FileExistsNeedsToBeMadeDelayStart(_path); }
		}

		/// <summary>
		///    Gets the name of the file. For E:\example\foo.txt, this would return "foo.txt".
		/// </summary>
		/// <value> The name of the file. </value>
		[NotNull]
		[PublicApi]
		public string FileName
		{
			get { return Path.GetFileName(_path._Absolute); }
		}

		/// <summary>
		///    Gets the base name of  the file. For E:\example\foo.txt, this would return "foo".
		/// </summary>
		[NotNull]
		[PublicApi]
		public string FileBaseName
		{
			get { return Path.GetFileNameWithoutExtension(_path._Absolute); }
		}

		/// <summary>
		///    Gets the file's extension. For E:\example\foo.txt, this would return ".txt".
		/// </summary>
		[NotNull]
		[PublicApi]
		public string Extension
		{
			get { return Path.GetExtension(_path._Absolute); }
		}

		/// <summary>
		///    Gets the full path to this file.
		/// </summary>
		[NotNull]
		[PublicApi]
		public FsPath FullPath
		{
			get { return _path; }
		}

		/// <summary>
		///    Regardless of the previous state of the file system, results in a file existing at this file's path with the
		///    contents given. This operation is revertable.
		/// </summary>
		/// <param name="newContents"> The new contents for the file </param>
		[NotNull]
		[PublicApi]
		public Task Overwrite([NotNull] string newContents)
		{
			return _allFiles._Disk.OverwriteNeedsToBeMadeDelayStart(_path, newContents);
		}

		/// <summary>
		///    Regardless of the previous state of the file system, results in a file existing at this file's path with the
		///    contents given. This operation is revertable.
		/// </summary>
		/// <param name="newContents"> The new contents for the file </param>
		[PublicApi]
		public void OverwriteBinary([NotNull] byte[] newContents)
		{
			_allFiles._Disk.OverwriteNeedsToBeMadeDelayStart(_path, newContents);
		}

		/// <summary>
		///    If the file exists, return its contents as a string.
		/// </summary>
		/// <returns> The entire contents of the file </returns>
		/// <exception cref="System.IO.FileNotFoundException">Thrown if the file does not exist.</exception>
		/// <exception cref="System.UnauthorizedAccessException">
		///    Thrown if this object's FullPath actually refers to a directory in
		///    the file system.
		/// </exception>
		[NotNull]
		[PublicApi]
		public Task<string> ReadAllText()
		{
			return _allFiles._Disk.TextContentsNeedsToBeMadeDelayStart(_path);
		}

		/// <summary>
		///    If the file exists, return its contents as a byte array.
		/// </summary>
		/// <returns> The entire contents of the file </returns>
		/// <exception cref="System.IO.FileNotFoundException">Thrown if the file does not exist.</exception>
		/// <exception cref="System.UnauthorizedAccessException">
		///    Thrown if this object's FullPath actually refers to a directory in
		///    the file system.
		/// </exception>
		[NotNull]
		[PublicApi]
		// ReSharper disable once ReturnTypeCanBeEnumerable.Global
		public IObservable<byte[]> ReadAllBytes()
		{
			return _allFiles._Disk.RawContentsNeedsToBeMadeDelayStart(_path);
		}

		/// <summary>
		///    Indicates whether two files represent the same path. They may come from different file systems and still be termed
		///    equal.
		/// </summary>
		/// <param name="other"> A file instance to compare with this object. </param>
		/// <returns> true if the two objects have the same path; otherwise, false. </returns>
		[PublicApi]
		public bool Equals([CanBeNull] FsFile other)
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
			return string.Format("File({0})", _path);
		}

		/// <summary>
		///    Indicates whether two files represent the same path. They may come from different file systems and still be termed
		///    equal.
		/// </summary>
		/// <param name="obj"> A file instance to compare with this object. </param>
		/// <returns> true if the two objects have the same path; otherwise, false. </returns>
		[PublicApi]
		public override bool Equals([CanBeNull] object obj)
		{
			return Equals(obj as FsFile);
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
		public static bool operator ==(FsFile left, FsFile right)
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
		public static bool operator !=(FsFile left, FsFile right)
		{
			return !Equals(left, right);
		}
	}
}
