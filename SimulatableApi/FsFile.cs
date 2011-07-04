using System;
using System.IO;
using JetBrains.Annotations;

namespace SimulatableApi
{
	public class FsFile : IEquatable<FsFile>
	{
		[NotNull] private readonly FileSystem _allFiles;
		[NotNull] private readonly FSPath _path;

		public FsFile([NotNull] FileSystem allFiles, [NotNull] FSPath path)
		{
			if (allFiles == null)
				throw new ArgumentNullException("allFiles");
			if (path == null)
				throw new ArgumentNullException("path");
			_allFiles = allFiles;
			_path = path;
		}

		public bool Equals(FsFile other)
		{
			if (ReferenceEquals(null, other))
				return false;
			if (ReferenceEquals(this, other))
				return true;
			return Equals(other._path, _path);
		}

		[NotNull]
		public FsDirectory ContainingFolder
		{
			get { return new FsDirectory(_allFiles, _path.Parent); }
		}

		public bool Exists
		{
			get { return _allFiles._Disk.FileExists(_path); }
		}

		[NotNull]
		public string FileName
		{
			get { return Path.GetFileName(_path.Absolute); }
		}

		[NotNull]
		public string Extension
		{
			get { return Path.GetExtension(_path.Absolute); }
		}

		[NotNull]
		public FSPath FullPath
		{
			get { return _path; }
		}

		/// <summary>
		/// Regardless of the previous state of the file system, results in a file existing at this file's path with the contents given. This operation is revertable.
		/// </summary>
		/// <param name="newContents">The new contents for the file</param>
		public void Overwrite([NotNull] string newContents)
		{
			var parent = ContainingFolder;
			if (!parent.Exists)
				parent.Create();
			_allFiles._Changes.Overwrote(_path);
			_allFiles._Disk.Overwrite(_path, newContents);
		}

		/// <summary>
		/// If the file exists, return its contents as a string.
		/// </summary>
		/// <returns>The entire contents of the file</returns>
		/// <exception cref="System.IO.FileNotFoundException">Thrown if the file does not exist.</exception>
		/// <exception cref="System.UnauthorizedAccessException">Thrown if this object's FullPath actually refers to a directory in the file system.</exception>
		public string ReadAllText()
		{
			return _allFiles._Disk.TextContents(_path);
		}

		public override string ToString()
		{
			return string.Format("File({0})", _path);
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as FsFile);
		}

		public override int GetHashCode()
		{
			return _path.GetHashCode();
		}

		public static bool operator ==(FsFile left, FsFile right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(FsFile left, FsFile right)
		{
			return !Equals(left, right);
		}
	}
}
