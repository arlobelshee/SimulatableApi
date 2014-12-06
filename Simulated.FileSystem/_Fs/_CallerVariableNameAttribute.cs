// SimulatableAPI
// File: _CallerVariableNameAttribute.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Simulated._Fs
{
	[AttributeUsage(AttributeTargets.Parameter)]
	internal class _CallerVariableNameAttribute : Attribute
	{
		[NotNull]
		public string Name { get; set; }

		public _CallerVariableNameAttribute([NotNull] [CallerMemberName] string name = "")
		{
			Name = name;
		}
	}
}
