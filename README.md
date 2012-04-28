SimulatableApi
==============

Implements several simulators for common dependencies. These abstractions make it easier to write highly-testable code.

File System
-----------

This class represents a transactional view on a file system. Actually, it represents a transactional view on an arbitrary storage medium, and this is where the Simulator comes in.

There are three ways to get a FileSystem. The first two, FileSystem.Real() and FileSystem.Simulated(), return a new view wrapped around some storage. The last, FileSystem.Clone() creates a view from another view. They share the same underlying storage, but each have their own change tracking and undo facilities.

The first thing to notice from just the above is that this is not your typical files and directories API. There is a single instance that represents the entire file system; file and directory objects are bound to a file system, and all the instances related to a single file system share consistent state.</description>
