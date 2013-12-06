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
		///    Indicates whether two files represent the same path. They may come from different file systems and still be termed
		///    equal.
		/// </summary>
		/// <param name="other"> A file instance to compare with this object. </param>
		/// <returns> true if the two objects have the same path; otherwise, false. </returns>
		public bool Equals(FsFile other)
		{
			if (ReferenceEquals(null, other))
				return false;
			if (ReferenceEquals(this, other))
				return true;
			return Equals(other._path, _path);
		}

		/// <summary>
		///    Gets the folder that contains this file.
		/// </summary>
		[NotNull]
		public FsDirectory ContainingFolder
		{
			get { return new FsDirectory(_allFiles, _path.Parent); }
		}

		/// <summary>
		///    Gets a value indicating whether this <see cref="FsFile" /> exists.
		/// </summary>
		/// <value> <c>true</c> if it exists; otherwise, <c>false</c> . </value>
		public Task<bool> Exists
		{
			get { return _allFiles._Disk.FileExists(_path); }
		}

		/// <summary>
		///    Gets the name of the file. For E:\example\foo.txt, this would return "foo.txt".
		/// </summary>
		/// <value> The name of the file. </value>
		[NotNull]
		public string FileName
		{
			get { return Path.GetFileName(_path.Absolute); }
		}

		/// <summary>
		///    Gets the base name of  the file. For E:\example\foo.txt, this would return "foo".
		/// </summary>
		public string FileBaseName
		{
			get { return Path.GetFileNameWithoutExtension(_path.Absolute); }
		}

		/// <summary>
		///    Gets the file's extension. For E:\example\foo.txt, this would return ".txt".
		/// </summary>
		[NotNull]
		public string Extension
		{
			get { return Path.GetExtension(_path.Absolute); }
		}

		/// <summary>
		///    Gets the full path to this file.
		/// </summary>
		[NotNull]
		public FsPath FullPath
		{
			get { return _path; }
		}

		/// <summary>
		///    Regardless of the previous state of the file system, results in a file existing at this file's path with the
		///    contents given. This operation is revertable.
		/// </summary>
		/// <param name="newContents"> The new contents for the file </param>
		public async Task Overwrite([NotNull] string newContents)
		{
			var parent = ContainingFolder;
			if (!await parent.Exists)
				await parent.EnsureExists();
			await _allFiles._Changes.Overwrote(_path);
			await _allFiles._Disk.Overwrite(_path, newContents);
		}

		/// <summary>
		///    Regardless of the previous state of the file system, results in a file existing at this file's path with the
		///    contents given. This operation is revertable.
		/// </summary>
		/// <param name="newContents"> The new contents for the file </param>
		public async Task OverwriteBinary([NotNull] byte[] newContents)
		{
			var parent = ContainingFolder;
			if (!await parent.Exists)
				await parent.EnsureExists();
			await _allFiles._Changes.Overwrote(_path);
			_allFiles._Disk.Overwrite(_path, newContents);
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
		public Task<string> ReadAllText()
		{
			return _allFiles._Disk.TextContents(_path);
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
		public Task<byte[]> ReadAllBytes()
		{
			return _allFiles._Disk.RawContents(_path);
		}

		/// <summary>
		///    Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		/// <returns> A <see cref="System.String" /> that represents this instance. </returns>
		public override string ToString()
		{
			return string.Format("File({0})", _path);
		}

		/// <summary>
		///    Indicates whether two files represent the same path. They may come from different file systems and still be termed
		///    equal.
		/// </summary>
		/// <param name="other"> A file instance to compare with this object. </param>
		/// <returns> true if the two objects have the same path; otherwise, false. </returns>
		public override bool Equals(object other)
		{
			return Equals(other as FsFile);
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
		///    Implements the operator ==. It is the same as Equals.
		/// </summary>
		/// <param name="left"> The left. </param>
		/// <param name="right"> The right. </param>
		/// <returns> The result of the operator. </returns>
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
		public static bool operator !=(FsFile left, FsFile right)
		{
			return !Equals(left, right);
		}
	}
}
