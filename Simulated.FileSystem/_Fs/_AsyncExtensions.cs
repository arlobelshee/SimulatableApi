using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Simulated._Fs
{
	static internal class _AsyncExtensions {
		[NotNull]
		public static Task<T> AsTask<T>([CanBeNull] this T result)
		{
			return Task.FromResult(result);
		}
	}
}