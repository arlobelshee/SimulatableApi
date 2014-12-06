// SimulatableAPI
// File: Require.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System;
using JetBrains.Annotations;

namespace Simulated._Fs
{
	internal static class _Require
	{
		public static void RequireNotNull([CanBeNull] this object possiblyNullValue, [NotNull] string callerVariableName)
		{
			if (possiblyNullValue == null)
				throw new ArgumentNullException(callerVariableName);
		}
	}
}
