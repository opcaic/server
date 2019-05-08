using System;
using OPCAIC.Utils;
using Xunit;

namespace OPCAIC.Utils.Test
{
	public class EnumerableExtensionTest
	{
		[Fact]
		public void ArgMin()
		{
			int[] values = { 1, 3, 5, -7 };
			Assert.Equal(1, values.ArgMin(v => v * v));
		}

		[Fact]
		public void ArgMax()
		{
			int[] values = { 1, 3, 5, -7 };
			Assert.Equal(-7, values.ArgMax(v => v * v));
		}

		[Fact]
		public void ArgMinMaxExceptions()
		{
			Assert.Throws<ArgumentNullException>(
				() => EnumerableExtensions.ArgMax(null, (int a) => a * a));
			Assert.Throws<ArgumentNullException>(
				() => new[] {1,2,3}.ArgMax<int, int>(null));
			Assert.Throws<InvalidOperationException>(
				() => new int[0].ArgMax((int a) => a * a));
		}
	}
}
