// SimulatableAPI
// File: _PathRoot.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System.IO;
using JetBrains.Annotations;

namespace Simulated._Fs
{
	public class _PathRoot
	{
		private readonly string _logicalName;
		private readonly string _absolutePath;

		public _PathRoot([NotNull] string logicalName, [NotNull] string absolutePath)
		{
			_logicalName = string.Format("{{{0}}}", logicalName);
			_absolutePath = absolutePath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
		}

		[NotNull]
		public string ToInternalPathStringThatMightDiscloseInformation([NotNull] string relativePath)
		{
			return ConcatPaths(_absolutePath, relativePath);
		}

		[NotNull]
		public string ToProgrammerVisibleString([NotNull] string relativePath)
		{
			return ConcatPaths(_logicalName, relativePath);
		}

		[NotNull]
		private string ConcatPaths([NotNull] string root, [NotNull] string relativePath)
		{
			return string.IsNullOrEmpty(relativePath)
				? (root.EndsWith(Path.VolumeSeparatorChar.ToString()) ? root + Path.DirectorySeparatorChar : root)
				: string.Format("{0}{1}{2}", root, Path.DirectorySeparatorChar, relativePath);
		}
	}
}
