using System;
using System.Linq.Expressions;
using Shouldly;
using Xunit;

namespace OPCAIC.Utils.Test
{
	public class RebindTest
	{
		[Fact]
		public void SimpleMapStillWorks()
		{
			Expression<Func<string, string>> expected = s => s + s;

			var actual = Rebind.Map(expected);

			var expectedFunc = expected.Compile();
			var actualFunc = actual.Compile();

			actualFunc("aaa").ShouldBe(expectedFunc("aaa"));
		}

		private struct TestClass
		{
			public string A { get; set; }
			public string B { get; set; }
		}

		[Fact]
		public void CollapsesMap()
		{
			Func<string, TestClass> expected =
				s => new TestClass
				{
					A = s + s,
					B = s
				};

			Expression<Func<string, string>> nested = s => s + s;
			Expression<Func<string, TestClass>> input =
				i => new TestClass // notice different param names in nested and input
				{
					A = Rebind.Invoke(i, nested),
					B = i
				};

			var actual = Rebind.Map(input).Compile();
			
			actual("abc").ShouldBe(expected("abc"));
		}
	}
}