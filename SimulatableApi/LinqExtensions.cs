using System.Collections.Generic;
using JetBrains.Annotations;

namespace System.Linq
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
