using FluentAssertions;
using JetBrains.Annotations;
using NUnit.Framework;

namespace SimulatableApi.StreamStore.Tests.zzTestHelpers
{
	internal static class _TestHelperExtensions
	{
		public static void ShouldExist(this FsDirectory dir)
		{
			dir.Exists.Should().BeTrue();
		}

		public static void ShouldContain([NotNull] this FsFile file, string contents)
		{
			file.Exists.Should().BeTrue();
			file.ReadAllText().Should().Be(contents);
		}

		public static void ShouldNotExist(this FsDirectory dir)
		{
			dir.Exists.Should().BeFalse();
		}

		public static void ShouldNotExist(this FsFile file)
		{
			file.Exists.Should().BeFalse();
		}
	}
}
