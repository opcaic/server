using System;
using System.Linq.Expressions;
using Shouldly;
using Xunit;

namespace OPCAIC.Utils.Test
{
	public class RebindTest
	{
		[Fact]
		public void TrivialMap_No_Invoke()
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
		public void SimpleSelect()
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

		private class OuterClass
		{
			public TestClass TestClass { get; set; }
		}

		[Fact]
		public void TrivialMap_InvokeOnly()
		{
			Func<OuterClass, string> expected = o => o.TestClass.A;

			Expression<Func<TestClass, string>> map = i => i.A;
			Expression<Func<OuterClass, string>> actualExpression =
				s => Rebind.Invoke(s.TestClass, map);

			var actual = Rebind.Map(actualExpression).Compile();

			var instance = new OuterClass {TestClass = new TestClass {A = "aaa", B = "bbb"}};

			actual(instance).ShouldBe(expected(instance));
		}
	}
}