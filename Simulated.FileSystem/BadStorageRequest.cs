// SimulatableAPI
// File: BadStorageRequest.cs
// 
// Copyright 2011, Arlo Belshee. All rights reserved. See LICENSE.txt for usage.

using System;
using System.IO;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace Simulated
{
	[Serializable]
	public class BadStorageRequest : IOException
	{
		public BadStorageRequest() {}
		public BadStorageRequest([NotNull] string message) : base(message) {}
		public BadStorageRequest([NotNull] string message, int hresult) : base(message, hresult) {}
		public BadStorageRequest([NotNull] string message, [NotNull] Exception innerException) : base(message, innerException) {}
		protected BadStorageRequest([NotNull] SerializationInfo info, StreamingContext context) : base(info, context) {}
	}
}
