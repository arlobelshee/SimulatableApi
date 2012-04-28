using System.Collections.Generic;
using JetBrains.Annotations;

// ReSharper disable CheckNamespace
namespace System.Linq
// ReSharper restore CheckNamespace
{
	public static class LinqExtensions
	{
		public static void Each<T>([NotNull] this IEnumerable<T> items, [NotNull] Action<T> op)
		{
			foreach (var item in items)
			{
				op(item);
			}
		}
	}
}
