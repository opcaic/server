using System;
using System.Runtime.InteropServices.WindowsRuntime;
using OPCAIC.Domain.Exceptions;
using OPCAIC.Domain.Infrastructure;
using Shouldly;
using Xunit;

namespace OPCAIC.Domain.Test
{
	public class EnumerationTest
	{
		public class TestEnum : Enumeration<TestEnum>
		{
			public static readonly TestEnum ValueA = Create<TestEnum>();
			public static readonly TestEnum ValueB = Create<TestEnum>();

			public static TestEnum PublicCreate(int id, string name)
			{
				return Create<TestEnum>(id, name);
			}
		}

		[Fact]
		public void CreatesValues()
		{
			TestEnum.ValueA.Id.ShouldBe(1);
			TestEnum.ValueA.Name.ShouldBe(nameof(TestEnum.ValueA));

			TestEnum.ValueB.Id.ShouldBe(2);
			TestEnum.ValueB.Name.ShouldBe(nameof(TestEnum.ValueB));

			TestEnum.AllValues.ShouldBe(new[] { TestEnum.ValueA, TestEnum.ValueB });
		}

		[Fact]
		public void ThrowsWhenDuplicateValueIsCreated()
		{
			Should.Throw<EnumerationException>(() => TestEnum.PublicCreate(TestEnum.ValueA.Id, "awefa"));
			Should.Throw<EnumerationException>(() => TestEnum.PublicCreate(42, nameof(TestEnum.ValueA)));
		}

		[Fact]
		public void ReturnsValueById()
		{
			var value = TestEnum.FromId(1);

			value.ShouldBe(TestEnum.ValueA);
			value.Id.ShouldBe(1);
			value.Name.ShouldBe(TestEnum.ValueA.Name);
		}

		[Fact]
		public void ReturnsValueByName()
		{
			var value = TestEnum.FromName(nameof(TestEnum.ValueB));

			value.ShouldBe(TestEnum.ValueB);
			value.Id.ShouldBe(2);
			value.Name.ShouldBe(TestEnum.ValueB.Name);
		}

		[Fact]
		public void ThrowsWhenNotFoundById()
		{
			Should.Throw<EnumerationException>(() => TestEnum.FromId(100));
		}

		[Fact]
		public void ThrowsWhenNotFoundByName()
		{
			Should.Throw<EnumerationException>(() => TestEnum.FromName("aewifawoefia"));
		}
	}
}
