using System;
using System.Linq;
using Xunit;

namespace OPCAIC.Utils.Test
{
	public class RequireTest
	{
		private static readonly string argName = "argName";

		[Theory]
		[InlineData(1, 1)]
		[InlineData(-1, 0)]
		public void IndexInRangeThrows(int i, int range)
		{
			var ex = Assert.Throws<ArgumentOutOfRangeException>(
				() => Require.ArgInRange(i, range, argName));
			Assert.Equal(argName, ex.ParamName);
		}

		[Theory]
		[InlineData(0, 1)]
		[InlineData(1, 2)]
		public void IndexInRangeDoesNotThrow(int i, int range)
		{
			Require.ArgInRange(i, range, argName);
		}

		[Fact]
		public void NonnegativeDoesNotThrow()
		{
			Require.Nonnegative(0, argName);
		}

		[Fact]
		public void NonnegativeThrows()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => Require.Nonnegative(-1, argName));
		}

		[Fact]
		public void NotEmptyDoesNotThrow()
		{
			Require.NotEmpty<InvalidOperationException>(new[] {1, 2, 3}, argName);
		}

		[Fact]
		public void NotEmptyThrows()
		{
			Assert.Throws<InvalidOperationException>(
				() => Require.NotEmpty<InvalidOperationException>(
					Enumerable.Empty<int>(), argName));
		}

		[Fact]
		public void NotNullDoesNotThrow()
		{
			Require.ArgNotNull(new object(), argName);
		}

		[Fact]
		public void NotNullThrows()
		{
			var ex = Assert.Throws<ArgumentNullException>(() => Require.ArgNotNull(null, argName));
			Assert.Equal(argName, ex.ParamName);
		}

		[Fact]
		public void GreaterThanDoesNotThrow()
		{
			Require.GreaterThan(1, 0, argName);
		}

		[Fact]
		public void GreaterThanThrows()
		{
			var ex = Assert.Throws<ArgumentOutOfRangeException>(() => Require.GreaterThan(1, 1, argName));
			Assert.Equal(argName, ex.ParamName);
		}

		[Fact]
		public void ThatDoesNotThrow()
		{
			Require.That<Exception>(true, argName);
		}

		[Fact]
		public void ThatThrows()
		{
			var ex = Assert.Throws<InvalidOperationException>(()
				=> Require.That<InvalidOperationException>(false, argName));

			Assert.Matches(argName, ex.ToString());
		}
	}
}