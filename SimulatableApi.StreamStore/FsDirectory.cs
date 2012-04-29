using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;

namespace Simulated
{
	/// <summary>
	/// 	Represents a folder in the underlying data store. This folder may or may not exist. This class exposes methods to create and delete folders, to manipulate their contents, and to ask for more information about the folder.
	/// </summary>
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
		/// 	Indicates whether two folders represent the same path. They may come from different file systems and still be termed equal.
		/// </summary>
		/// <param name="other"> A directory instance to compare with this object. </param>
		/// <returns> true if the two objects have the same path; otherwise, false. </returns>
		public bool Equals(FsDirectory other)
		{
			if (ReferenceEquals(null, other))
				return false;
			if (ReferenceEquals(this, other))
				return true;
			return Equals(other._path, _path);
		}

		/// <summary>
		/// 	Gets a value indicating whether this <see cref="FsDirectory" /> exists.
		/// </summary>
		/// <value> <c>true</c> if it exists; otherwise, <c>false</c> . </value>
		public bool Exists
		{
			get { return _allFiles._Disk.DirExists(_path); }
		}

		/// <summary>
		/// 	Gets the path to this directory.
		/// </summary>
		[NotNull]
		public FsPath Path
		{
			get { return _path; }
		}

		/// <summary>
		/// 	Gets the directory that contains this directory.
		/// </summary>
		[NotNull]
		public FsDirectory Parent
		{
			get { return new FsDirectory(_allFiles, _path.Parent); }
		}

		/// <summary>
		/// 	Gets a drectory instance that represents a sub-directory of this directory.
		/// </summary>
		/// <param name="subdirName"> Name of the subdir. </param>
		/// <returns> the subdir as a directory object </returns>
		[NotNull]
		public FsDirectory Dir(string subdirName)
		{
			return new FsDirectory(_allFiles, _path/subdirName);
		}

		/// <summary>
		/// 	Regardless of the previous state of the file system, results in a directory existing at this object's Path. This operation is revertable.
		/// </summary>
		public void Create()
		{
			if (Exists)
				return;
			_AllMissingDirectoriesInPathFromBottomUp().Reverse().Each(dir => _allFiles._Changes.CreatedDirectory(new FsPath(dir)));
			_allFiles._Disk.CreateDir(_path);
		}

		/// <summary>
		/// Gets a file object for a file in this directory.
		/// </summary>
		/// <param name="fileName">Name of the file.</param>
		/// <returns>a file in this directory</returns>
		public FsFile File(string fileName)
		{
			return new FsFile(_allFiles, _path/fileName);
		}

		/// <summary>
		/// 	Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		/// <returns> A <see cref="System.String" /> that represents this instance. </returns>
		public override string ToString()
		{
			return string.Format("Directory({0})", _path);
		}

		/// <summary>
		/// 	Indicates whether two folders represent the same path. They may come from different file systems and still be termed equal.
		/// </summary>
		/// <param name="other"> A directory instance to compare with this object. </param>
		/// <returns> true if the two objects have the same path; otherwise, false. </returns>
		public override bool Equals(object obj)
		{
			return Equals(obj as FsDirectory);
		}

		/// <summary>
		/// 	Returns a hash code for this instance.
		/// </summary>
		/// <returns> A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. </returns>
		public override int GetHashCode()
		{
			return _path.GetHashCode();
		}

		/// <summary>
		/// 	Implements the operator ==. It is the same as <see cref="Equals" /> .
		/// </summary>
		/// <param name="left"> The left. </param>
		/// <param name="right"> The right. </param>
		/// <returns> The result of the operator. </returns>
		public static bool operator ==(FsDirectory left, FsDirectory right)
		{
			return Equals(left, right);
		}

		/// <summary>
		/// 	Implements the operator !=. It is the same as !(left == right)
		/// </summary>
		/// <param name="left"> The left. </param>
		/// <param name="right"> The right. </param>
		/// <returns> The result of the operator. </returns>
		public static bool operator !=(FsDirectory left, FsDirectory right)
		{
			return !Equals(left, right);
		}

		[NotNull]
		private IEnumerable<string> _AllMissingDirectoriesInPathFromBottomUp()
		{
			var dir = new DirectoryInfo(_path.Absolute);
			DirectoryInfo root = dir.Root;
			while (!dir.Exists && dir != root)
			{
				yield return dir.FullName;
				dir = dir.Parent;
			}
		}
	}
}
