using System;
using Xunit;

namespace OPCAIC.Utils.Test
{
	public class EnumerableExtensionTest
	{
		[Fact]
		public void ArgExtremeOrDefault()
		{
			Exception[] values = { };
			Assert.Null(values.ArgMinOrDefault(e => e.Message));
			Assert.Null(values.ArgMaxOrDefault(e => e.Message));
		}

		[Fact]
		public void ArgMax()
		{
			int[] values = {1, 3, 5, -7};
			Assert.Equal(-7, values.ArgMax(v => v * v));
		}

		[Fact]
		public void ArgMin()
		{
			int[] values = {1, 3, 5, -7};
			Assert.Equal(1, values.ArgMin(v => v * v));
		}

		[Fact]
		public void ArgMinMaxExceptions()
		{
			Func<double, double> selector = Math.Sqrt;
			Assert.Throws<ArgumentNullException>(
				() => EnumerableExtensions.ArgMax(null, selector));
			Assert.Throws<ArgumentNullException>(
				() => new[] {1.0, 2.0, 3.0}.ArgMax<double, double>(null));
			Assert.Throws<InvalidOperationException>(
				() => new double[0].ArgMax(selector));
		}
	}
}