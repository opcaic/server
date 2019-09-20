using System;
using System.Linq.Expressions;
using Shouldly;
using Xunit;

namespace OPCAIC.Utils.Test
{
	public class ExpressionExtensionsTest
	{
		[Fact]
		public void Combines_And()
		{
			Expression<Func<string, bool>> first = s => s.Length > 0;

			// note intentional different name of the parameter
			Expression<Func<string, bool>> second = s2 => s2.Length < 3;

			var combined = first.And(second).Compile();

			combined("too long").ShouldBe(false);
			combined("ok").ShouldBe(true);
			combined("").ShouldBe(false);
		}
	}
}