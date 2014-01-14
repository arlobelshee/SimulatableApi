// SimulatableAPI
// File: _StorageTransform.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Simulated._Fs
{
	internal abstract class _StorageSink
	{
		public abstract bool IsTrackingChanges { get; }

		[NotNull]
		public abstract Task CommitAll();

		[NotNull]
		public abstract Task RevertAll();

		public abstract void CreatedDirectory([NotNull] FsPath path);

		[NotNull]
		public abstract Task Overwrote([NotNull] FsPath path);

		[NotNull]
		public abstract Task DeletedDirectory([NotNull] FsPath path);
	}

	internal abstract class _StorageTransform : _StorageSink
	{
		[CanBeNull] private readonly _StorageSink _next;

		protected _StorageTransform([NotNull] _StorageSink next)
		{
			_next = next;
		}

		public override bool IsTrackingChanges
		{
			get { return _next != null && _next.IsTrackingChanges; }
		}

		public override Task CommitAll()
		{
			return _next != null ? _next.CommitAll() : _Undo.CompletedTask;
		}

		public override Task RevertAll()
		{
			return _next != null ? _next.RevertAll() : _Undo.CompletedTask;
		}

		public override void CreatedDirectory(FsPath path)
		{
			if (_next != null)
				_next.CreatedDirectory(path);
		}

		public override Task Overwrote(FsPath path)
		{
			return _next != null ? _next.Overwrote(path) : _Undo.CompletedTask;
		}

		public override Task DeletedDirectory(FsPath path)
		{
			return _next != null ? _next.DeletedDirectory(path) : _Undo.CompletedTask;
		}
	}
}
