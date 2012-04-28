using System;
using System.IO;
using JetBrains.Annotations;

namespace SimulatableApi.StreamStore
{
	public class FSPath : IEquatable<FSPath>
	{
		[NotNull] private readonly string _absolutePath;

		public FSPath([NotNull] string absolutePath)
		{
			if (string.IsNullOrEmpty(absolutePath))
				throw new ArgumentNullException("absolutePath", "A path cannot be null or empty.");
			_absolutePath = absolutePath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
		}

		public bool Equals(FSPath other)
		{
			if (ReferenceEquals(null, other))
				return false;
			if (ReferenceEquals(this, other))
				return true;
			return Equals(other._absolutePath, _absolutePath);
		}

		[NotNull]
		public static FSPath TempFolder
		{
			get { return new FSPath(Path.GetTempPath()); }
		}

		[NotNull]
		public string Absolute
		{
			get { return _absolutePath; }
		}

		[NotNull]
		public FSPath Parent
		{
			get
			{
				var parent = Path.GetDirectoryName(_absolutePath);
				if (string.IsNullOrEmpty(parent))
					throw new InvalidOperationException("The root directory does not have a parent.");
				return new FSPath(parent);
			}
		}

		public bool IsRoot
		{
			get { return Path.GetPathRoot(_absolutePath) == _absolutePath; }
		}

		[NotNull]
		public static FSPath operator /([NotNull] FSPath self, [NotNull] string nextStep)
		{
			return new FSPath(Path.Combine(self._absolutePath, nextStep));
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as FSPath);
		}

		public override int GetHashCode()
		{
			return _absolutePath.GetHashCode();
		}

		public static bool operator ==(FSPath left, FSPath right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(FSPath left, FSPath right)
		{
			return !Equals(left, right);
		}

		public override string ToString()
		{
			return _absolutePath;
		}
	}
}
