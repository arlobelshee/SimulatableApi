using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;

namespace SimulatableApi
{
	public class FsDirectory : IEquatable<FsDirectory>
	{
		[NotNull] private readonly FileSystem _allFiles;
		[NotNull] private readonly FSPath _path;

		public FsDirectory([NotNull] FileSystem allFiles, [NotNull] FSPath path)
		{
			if (path == null)
				throw new ArgumentNullException("path");
			if (allFiles == null)
				throw new ArgumentNullException("allFiles");
			_path = path;
			_allFiles = allFiles;
		}

		public bool Equals(FsDirectory other)
		{
			if (ReferenceEquals(null, other))
				return false;
			if (ReferenceEquals(this, other))
				return true;
			return Equals(other._path, _path);
		}

		public bool Exists
		{
			get { return _allFiles._Disk.DirExists(_path); }
		}

		[NotNull]
		public FSPath Path
		{
			get { return _path; }
		}

		[NotNull]
		public FsDirectory Parent
		{
			get { return new FsDirectory(_allFiles, _path.Parent); }
		}

		public FsDirectory Dir(string subdirName)
		{
			return new FsDirectory(_allFiles, _path/subdirName);
		}

		/// <summary>
		/// Regardless of the previous state of the file system, results in a directory existing at this object's Path. This operation is revertable.
		/// </summary>
		public void Create()
		{
			if (Exists)
				return;
			_AllMissingDirectoriesInPathFromBottomUp().Reverse().Each(dir => _allFiles._Changes.CreatedDirectory(new FSPath(dir)));
			_allFiles._Disk.CreateDir(_path);
		}

		[NotNull]
		private IEnumerable<string> _AllMissingDirectoriesInPathFromBottomUp()
		{
			var dir = new DirectoryInfo(_path.Absolute);
			var root = dir.Root;
			while (!dir.Exists && dir != root)
			{
				yield return dir.FullName;
				dir = dir.Parent;
			}
		}

		public FsFile File(string fileName)
		{
			return new FsFile(_allFiles, _path/fileName);
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as FsDirectory);
		}

		public override int GetHashCode()
		{
			return _path.GetHashCode();
		}

		public static bool operator ==(FsDirectory left, FsDirectory right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(FsDirectory left, FsDirectory right)
		{
			return !Equals(left, right);
		}

		public override string ToString()
		{
			return string.Format("Directory({0})", _path);
		}
	}
}
