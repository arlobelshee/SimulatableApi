SimulatableApi
==============

Implements ports and adaptors (including simulators) for common dependencies. These abstractions make it easier to write highly-testable code.

The project page is at http://arlobelshee.github.com/SimulatableApi/. It describes the abstractions used in this library and which simulators I currently intend to write. It also gives much more complete documentation for each completed simulator.

The rest of this readme is just a teaser to let you know what's in the library.

File System
-----------

There is a port for a transactional stream store. It has two adaptors:

 * Disk-backed
 * Memory-backed

This lets you easily code using stream I/O without caring where the stream is stored, even when you have to create streams or load streams from a path.
