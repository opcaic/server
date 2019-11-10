using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
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

		class ClassWithStaticMemberExpression
		{
			public int Property { get; set; }

			public static Expression<Func<ClassWithStaticMemberExpression, int>> PropertyExpression =
				e => e.Property;
		}

		[Fact]
		public void StaticClassMember_Expression()
		{
			var instance = new ClassWithStaticMemberExpression() {Property = 5};

			var selector = Rebind.Map((ClassWithStaticMemberExpression i)
				=> Rebind.Invoke(i, ClassWithStaticMemberExpression.PropertyExpression)).Compile();

			selector(instance).ShouldBe(5);
		}

		class ClassWithCollection
		{
			public IList<TestClass> Collection { get; set; }
		}

		[Fact]
		public void ExtensionMethod()
		{
			Expression<Func<TestClass, bool>> filter = i => i.A.Length != 1;
			var instance = new ClassWithCollection
			{
				Collection = new List<TestClass>
				{
					new TestClass {A = ""}, new TestClass {A = "1"}
				}
			};

			var compiled = Rebind.Map<ClassWithCollection,int>(i
				=> i.Collection.Count(e => Rebind.Invoke(e, filter))).Compile();

			compiled(instance).ShouldBe(1);
		}

		[Fact]
		public void Recursive()
		{
			var instance = new OuterClass
			{
				TestClass = new TestClass
				{
					A = "a",
					B = "b"
				}
			};

			Expression<Func<OuterClass, TestClass>> outer = a => a.TestClass;
			Expression<Func<TestClass, string>> inner = a => a.A;

			var composed = Rebind.Map((OuterClass o) => new
			{
				A = Rebind.Invoke(Rebind.Invoke(o, outer), inner)
			}).Compile();

			composed(instance).A.ShouldBe(instance.TestClass.A);
		}
	}
}