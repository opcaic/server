using System.ComponentModel.DataAnnotations;
using OPCAIC.ApiService.ModelValidationHandling.Attributes;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.ApiService.Test.ModelValidationHandling.Attributes
{
	public class ApiRangeAttributeTest : ApiAttributeTestBase
	{
		/// <inheritdoc />
		public ApiRangeAttributeTest(ITestOutputHelper output) : base(output)
		{
		}

		[Theory]
		[InlineData(0, double.MinValue, double.MaxValue)]
		[InlineData(0, 1, 2)]
		[InlineData(0, -2, -1)]
		[InlineData(0, -10, 10)]
		[InlineData(42, -420, 420)]
		public void ReturnsCorrectResult(double value, double minimum, double maximum)
		{
			var apiAttribute = new ApiRangeAttribute(minimum, maximum);
			var originalAttribute = new RangeAttribute(minimum, maximum);

			AssertSameResult(value, apiAttribute, originalAttribute);
		}
	}
}