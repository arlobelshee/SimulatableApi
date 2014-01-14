// SimulatableAPI
// File: Undo.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Simulated._Fs
{
	internal class _Undo
	{
		public static readonly Task CompletedTask = true.AsImmediateTask();

		public virtual bool IsTrackingChanges
		{
			get { return false; }
		}

		[NotNull]
		public virtual Task CommitAll()
		{
			return CompletedTask;
		}

		[NotNull]
		public virtual Task RevertAll()
		{
			return CompletedTask;
		}

		public virtual void CreatedDirectory([NotNull] FsPath path) {}

		[NotNull]
		public virtual Task Overwrote([NotNull] FsPath path)
		{
			return CompletedTask;
		}

		[NotNull]
		public virtual Task DeletedDirectory([NotNull] FsPath path)
		{
			return CompletedTask;
		}
	}
}
