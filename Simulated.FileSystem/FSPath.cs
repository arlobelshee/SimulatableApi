using System;
using System.IO;
using JetBrains.Annotations;

namespace Simulated
{
	/// <summary>
	/// <see cref="FsPath"/> represents an absolute path.
	/// 
	/// This class allows composition of paths. You can use the / operator to add a relative path to an existing <see cref="FsPath"/> in order to get a new <see cref="FsPath"/>.
	/// 
	/// The path is agnostic to a file system. You could ask a file on one file system for its path, then use that path to create a file on another file system.
	/// </summary>
	public class FsPath : IEquatable<FsPath>
	{
		[NotNull] private readonly string _absolutePath;

		/// <summary>
		/// Initializes a new instance of the <see cref="FsPath"/> class.
		/// </summary>
		/// <param name="absolutePath">The path. It must be absolute.</param>
		/// <exception cref="ArgumentNullException">Throws when absolute path is null or empty.</exception>
		/// <exception cref="ArgumentException">Throws when absolute path is a relative path.</exception>
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

		/// <summary>
		/// Indicates whether the current object represents the same path as another object.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		public bool Equals(FsPath other)
		{
			if (ReferenceEquals(null, other))
				return false;
			if (ReferenceEquals(this, other))
				return true;
			return Equals(other._absolutePath, _absolutePath);
		}

		/// <summary>
		/// Gets the path for the temp folder.
		/// </summary>
		[NotNull]
		public static FsPath TempFolder
		{
			get { return new FsPath(Path.GetTempPath()); }
		}

		/// <summary>
		/// Gets a string containing the absolute path that this object represents.
		/// </summary>
		[NotNull]
		public string Absolute
		{
			get { return _absolutePath; }
		}

		/// <summary>
		/// Gets the parent directory for this path.
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown when you ask for the parent of a drive root.</exception>
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

		/// <summary>
		/// Gets a value indicating whether this instance is a drive root.
		/// </summary>
		/// <value>
		///   <c>true</c> if this instance is a drive root; otherwise, <c>false</c>.
		/// </value>
		public bool IsRoot
		{
			get { return Path.GetPathRoot(_absolutePath) == _absolutePath; }
		}

		/// <summary>
		/// Combines this absolute path with a relative path to create a new absolute path.
		/// </summary>
		/// <param name="self">The absolute path.</param>
		/// <param name="nextStep">The relative path from that base.</param>
		/// <returns>
		/// An absolute path.
		/// </returns>
		[NotNull]
		public static FsPath operator /([NotNull] FsPath self, [NotNull] string nextStep)
		{
			return new FsPath(Path.Combine(self._absolutePath, nextStep));
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			return _absolutePath;
		}

		/// <summary>
		/// Indicates whether the current object represents the same path as another object.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		public override bool Equals(object obj)
		{
			return Equals(obj as FsPath);
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode()
		{
			return _absolutePath.GetHashCode();
		}

		/// <summary>
		/// 	Implements the operator ==. It is the same as <see cref="Equals" /> .
		/// </summary>
		/// <param name="left"> The left. </param>
		/// <param name="right"> The right. </param>
		/// <returns> The result of the operator. </returns>
		public static bool operator ==(FsPath left, FsPath right)
		{
			return Equals(left, right);
		}

		/// <summary>
		/// 	Implements the operator !=. It is the same as !(left == right)
		/// </summary>
		/// <param name="left"> The left. </param>
		/// <param name="right"> The right. </param>
		/// <returns> The result of the operator. </returns>
		public static bool operator !=(FsPath left, FsPath right)
		{
			return !Equals(left, right);
		}
	}
}
