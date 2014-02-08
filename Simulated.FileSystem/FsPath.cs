// SimulatableAPI
// File: FsPath.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using JetBrains.Annotations;

namespace Simulated
{
	/// <summary>
	///    <see cref="FsPath" /> represents an absolute path.
	///    This class allows composition of paths. You can use the / operator to add a relative path to an existing
	///    <see cref="FsPath" /> in order to get a new <see cref="FsPath" />.
	///    The path is agnostic to a file system. You could ask a file on one file system for its path, then use that path to
	///    create a file on another file system.
	/// </summary>
	[PublicApi]
	public class FsPath : IEquatable<FsPath>
	{
		[NotNull] private readonly string _absolutePath;

		/// <summary>
		///    Initializes a new instance of the <see cref="FsPath" /> class.
		/// </summary>
		/// <param name="absolutePath">The path. It must be absolute.</param>
		/// <exception cref="ArgumentNullException">Throws when absolute path is null or empty.</exception>
		/// <exception cref="ArgumentException">Throws when absolute path is a relative path.</exception>
		public FsPath([NotNull] string absolutePath)
		{
			if (string.IsNullOrEmpty(absolutePath))
				throw new ArgumentNullException("absolutePath", UserMessages.ErrorPathCannotBeNullOrEmpty);
			if (!absolutePath.Substring(1, 2)
				.Equals(":\\"))
				throw new ArgumentException(string.Format(UserMessages.ErrorPathMustBeAbsolute, absolutePath), "absolutePath");
			_absolutePath = absolutePath;
			if (absolutePath.Length > 3)
				_absolutePath = absolutePath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
		}

		/// <summary>
		///    Gets the path for the temp folder.
		/// </summary>
		[PublicApi]
		[NotNull]
		public static FsPath TempFolder
		{
			get { return new FsPath(Path.GetTempPath()); }
		}

		/// <summary>
		///    Gets a string containing the absolute path that this object represents.
		/// </summary>
		[PublicApi]
		[NotNull]
		public string Absolute
		{
			get { return _absolutePath; }
		}

		/// <summary>
		///    Gets the parent directory for this path.
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown when you ask for the parent of a drive root.</exception>
		[PublicApi]
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
		///    Gets a value indicating whether this instance is a drive root.
		/// </summary>
		/// <value>
		///    <c>true</c> if this instance is a drive root; otherwise, <c>false</c>.
		/// </value>
		[PublicApi]
		public bool IsRoot
		{
			get { return Path.GetPathRoot(_absolutePath) == _absolutePath; }
		}

		/// <summary>
		///    Combines this absolute path with a relative path to create a new absolute path.
		/// </summary>
		/// <param name="self">The absolute path.</param>
		/// <param name="nextStep">The relative path from that base.</param>
		/// <returns>
		///    An absolute path.
		/// </returns>
		[PublicApi]
		[NotNull]
		public static FsPath operator /([NotNull] FsPath self, [NotNull] string nextStep)
		{
			return new FsPath(Path.Combine(self._absolutePath, nextStep));
		}

		[PublicApi]
		public bool IsAncestorOf([NotNull] FsPath possibleDescendent, bool descendentIsDirectory)
		{
			var ancestorPath = _AllDirectoryNamesInOrder(true);
			var descendentPath = possibleDescendent._AllDirectoryNamesInOrder(descendentIsDirectory);
			return descendentPath.Length >= ancestorPath.Length && ancestorPath.Zip(descendentPath, string.Equals)
				.All(b => b);
		}

		[PublicApi]
		[NotNull]
		public FsPath ReplaceAncestor([NotNull] FsPath currentAncestor, [NotNull] FsPath newAncestor, bool descendentIsDirectory)
		{
			var myPath = _AllDirectoryNamesInOrder(descendentIsDirectory);
			var rootElementsToTrim = currentAncestor._AllDirectoryNamesInOrder(true)
				.Length;
			var myUniquePathElements = myPath.Skip(rootElementsToTrim);
			if (!descendentIsDirectory)
			{
				myUniquePathElements = myUniquePathElements.Concat(new[] {Path.GetFileName(_absolutePath)});
			}
			return newAncestor/Path.Combine(myUniquePathElements.ToArray());
		}

		[NotNull]
		private string[] _AllDirectoryNamesInOrder(bool descendentIsDirectory)
		{
			var deepestDirectory = descendentIsDirectory ? _absolutePath : Path.GetDirectoryName(_absolutePath);
			Debug.Assert(deepestDirectory != null, "deepestDirectory != null");
			return deepestDirectory.Split(new[] {Path.DirectorySeparatorChar}, StringSplitOptions.RemoveEmptyEntries);
		}

		/// <summary>
		///    Indicates whether the current object represents the same path as another object.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		///    true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
		/// </returns>
		[PublicApi]
		public bool Equals([CanBeNull] FsPath other)
		{
			if (ReferenceEquals(null, other))
				return false;
			if (ReferenceEquals(this, other))
				return true;
			return Equals(other._absolutePath, _absolutePath);
		}

		/// <summary>
		///    Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		/// <returns>
		///    A <see cref="System.String" /> that represents this instance.
		/// </returns>
		[PublicApi]
		public override string ToString()
		{
			return _absolutePath;
		}

		/// <summary>
		///    Indicates whether the current object represents the same path as another object.
		/// </summary>
		/// <param name="obj">An object to compare with this object.</param>
		/// <returns>
		///    true if the current object is equal to the <paramref name="obj" /> parameter; otherwise, false.
		/// </returns>
		[PublicApi]
		public override bool Equals([CanBeNull] object obj)
		{
			return Equals(obj as FsPath);
		}

		/// <summary>
		///    Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		///    A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
		/// </returns>
		[PublicApi]
		public override int GetHashCode()
		{
			return _absolutePath.GetHashCode();
		}

		/// <summary>
		///    Implements the operator ==. It is the same as Equals /> .
		/// </summary>
		/// <param name="left"> The left. </param>
		/// <param name="right"> The right. </param>
		/// <returns> The result of the operator. </returns>
		[PublicApi]
		public static bool operator ==(FsPath left, FsPath right)
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
		public static bool operator !=(FsPath left, FsPath right)
		{
			return !Equals(left, right);
		}
	}
}
