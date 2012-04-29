using System;
using System.IO;
using JetBrains.Annotations;

namespace SimulatableApi.StreamStore
{
	public class FsPath : IEquatable<FsPath>
	{
		[NotNull] private readonly string _absolutePath;

		public FsPath([NotNull] string absolutePath)
		{
			if (string.IsNullOrEmpty(absolutePath))
				throw new ArgumentNullException("absolutePath", "A path cannot be null or empty.");
			if(!absolutePath.Substring(1,2).Equals(":\\"))
				throw new ArgumentException(string.Format("The path must be absolute. '{0}' is not an absolute path.", absolutePath), "absolutePath");
			_absolutePath = absolutePath;
			if(absolutePath.Length>3)
				_absolutePath = absolutePath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
		}

		public bool Equals(FsPath other)
		{
			if (ReferenceEquals(null, other))
				return false;
			if (ReferenceEquals(this, other))
				return true;
			return Equals(other._absolutePath, _absolutePath);
		}

		[NotNull]
		public static FsPath TempFolder
		{
			get { return new FsPath(Path.GetTempPath()); }
		}

		[NotNull]
		public string Absolute
		{
			get { return _absolutePath; }
		}

		[NotNull]
		public FsPath Parent
		{
			get
			{
				var parent = Path.GetDirectoryName(_absolutePath);
				if (string.IsNullOrEmpty(parent))
					throw new InvalidOperationException(string.Format("'{0}' is a drive root. It does not have a parent.", _absolutePath));
				return new FsPath(parent);
			}
		}

		public bool IsRoot
		{
			get { return Path.GetPathRoot(_absolutePath) == _absolutePath; }
		}

		[NotNull]
		public static FsPath operator /([NotNull] FsPath self, [NotNull] string nextStep)
		{
			return new FsPath(Path.Combine(self._absolutePath, nextStep));
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as FsPath);
		}

		public override int GetHashCode()
		{
			return _absolutePath.GetHashCode();
		}

		public static bool operator ==(FsPath left, FsPath right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(FsPath left, FsPath right)
		{
			return !Equals(left, right);
		}

		public override string ToString()
		{
			return _absolutePath;
		}
	}
}
