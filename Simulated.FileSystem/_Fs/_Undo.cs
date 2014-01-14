// SimulatableAPI
// File: Undo.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Simulated._Fs
{
	internal class _Undo : _StorageTransform
	{
		[NotNull] public static readonly Task CompletedTask = true.AsImmediateTask();

		public _Undo(): base(null) {}
	}
}
