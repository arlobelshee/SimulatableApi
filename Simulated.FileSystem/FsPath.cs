// SimulatableAPI
// File: FsPath.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Simulated._Fs;

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
		[NotNull] private readonly _PathRoot _root;
		[NotNull] private readonly string _relativePath;
		[NotNull] private static readonly Lazy<_PathRoot> TempFolderRoot = new Lazy<_PathRoot>(() => new _PathRoot("Temp folder", Path.GetTempPath()));
		[NotNull] private static readonly Lazy<_PathRoot> PrimaryDriveRoot = new Lazy<_PathRoot>(() => new _PathRoot("Primary drive", "C:"));

		/// <summary>
		///    Initializes a new instance of the <see cref="FsPath" /> class.
		/// </summary>
		/// <param name="absolutePath">The path. It must be absolute.</param>
		/// <exception cref="ArgumentNullException">Throws when absolute path is null or empty.</exception>
		/// <exception cref="ArgumentException">Throws when absolute path is a relative path.</exception>
		public FsPath([NotNull] string absolutePath) : this(PrimaryDriveRoot.Value, string.IsNullOrEmpty(absolutePath) ? absolutePath : absolutePath.Substring(3))
		{
			if (string.IsNullOrEmpty(absolutePath))
				throw new ArgumentNullException("absolutePath", UserMessages.ErrorPathMustHaveRoot);
			if (!absolutePath.Substring(1, 2)
				.Equals(":\\"))
				throw new ArgumentException(string.Format(UserMessages.ErrorPathMustBeAbsolute, absolutePath), "absolutePath");
		}

		public FsPath([NotNull] _PathRoot root, [CanBeNull] string relativePath)
		{
			if (root == null)
				throw new ArgumentNullException("root", UserMessages.ErrorPathMustHaveRoot);
			_root = root;
			_relativePath = (relativePath ?? string.Empty).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
		}

		/// <summary>
		///    Gets the path for the temp folder.
		/// </summary>
		[PublicApi]
		[NotNull]
		public static FsPath TempFolder
		{
			get { return new FsPath(TempFolderRoot.Value, string.Empty); }
		}

		/// <summary>
		///    Gets a string containing the absolute path that this object represents.
		/// </summary>
		[NotNull]
		internal string _Absolute
		{
			get { return _root.ToInternalPathStringThatMightDiscloseInformation(_relativePath); }
		}

		/// <summary>
		///    Gets the parent directory for this path.
		/// </summary>
		/// <exception cref="BadStorageRequest">Thrown when you ask for the parent of a root.</exception>
		[PublicApi]
		[NotNull]
		public FsPath Parent
		{
			get
			{
				if (string.IsNullOrEmpty(_relativePath))
					throw new BadStorageRequest(string.Format(UserMessages.ErrorPathHasNoParent, ToString()));
				var parent = Path.GetDirectoryName(_relativePath);
				return new FsPath(_root, parent);
			}
		}

		/// <summary>
		///    Gets a value indicating whether this instance is a root. A root is any path for which there is no parent. For
		///    example, the temp folder is a root because you are not allowed to ask for its parent.
		/// </summary>
		/// <value>
		///    <c>true</c> if this instance is a root; otherwise, <c>false</c>.
		/// </value>
		[PublicApi]
		public bool IsRoot
		{
			get { return string.IsNullOrEmpty(_relativePath); }
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
			return new FsPath(self._root, Path.Combine(self._relativePath, nextStep));
		}

		[PublicApi]
		public bool IsAncestorOf([NotNull] FsPath possibleDescendent, bool descendentIsDirectory)
		{
			var ancestorPath = _AllDirectoryNamesInOrder(true);
			var descendentPath = possibleDescendent._AllDirectoryNamesInOrder(descendentIsDirectory);
			return descendentPath.Length >= ancestorPath.Length && ancestorPath.Zip(descendentPath, string.Equals)
				.All(b => b);
		}

		[NotNull]
		internal FsPath _ReplaceAncestor([NotNull] FsPath currentAncestor, [NotNull] FsPath newAncestor, bool descendentIsDirectory)
		{
			var myPath = _AllDirectoryNamesInOrder(descendentIsDirectory);
			var rootElementsToTrim = currentAncestor._AllDirectoryNamesInOrder(true)
				.Length;
			var myUniquePathElements = myPath.Skip(rootElementsToTrim);
			if (!descendentIsDirectory)
			{
				myUniquePathElements = myUniquePathElements.Concat(new[] {Path.GetFileName(_relativePath)});
			}
			return newAncestor/Path.Combine(myUniquePathElements.ToArray());
		}

		[NotNull]
		private string[] _AllDirectoryNamesInOrder(bool descendentIsDirectory)
		{
			var deepestDirectory = descendentIsDirectory ? _relativePath : Path.GetDirectoryName(_relativePath);
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
			return Equals(other._root, _root) && Equals(other._relativePath, _relativePath);
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
			return _root.ToProgrammerVisibleString(_relativePath);
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
			unchecked
			{
				return (_root.GetHashCode()*397) ^ _relativePath.GetHashCode();
			}
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
