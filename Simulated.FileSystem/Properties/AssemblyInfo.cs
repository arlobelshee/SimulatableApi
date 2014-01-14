// SimulatableAPI
// File: AssemblyInfo.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.

[assembly: AssemblyTitle("Simulated.FileSystem")]
[assembly:
	AssemblyDescription(
		"This package provides a transactional view of a file system and an in-memory simulator for that view. This makes it easy to test code that interacts with the file system."
		)]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("arlobelshee")]
[assembly: AssemblyProduct("Simulated.FileSystem")]
[assembly: AssemblyCopyright("Copyright © Arlo Belshee 2011")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: InternalsVisibleTo("Simulated.FileSystem.Tests")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.

[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM

[assembly: Guid("03adc7a4-6f63-4be2-a8db-ce31ddd0413b")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]

[assembly: AssemblyVersion("0.5.1.0")]
[assembly: AssemblyFileVersion("0.5.1.0")]
