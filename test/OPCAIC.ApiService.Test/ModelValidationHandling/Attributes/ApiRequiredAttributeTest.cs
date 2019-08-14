using System.ComponentModel.DataAnnotations;
using OPCAIC.ApiService.ModelValidationHandling.Attributes;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.ApiService.Test.ModelValidationHandling.Attributes
{
	public class ApiRequiredAttributeTest : ApiAttributeTestBase
	{
		/// <inheritdoc />
		public ApiRequiredAttributeTest(ITestOutputHelper output) : base(output)
		{
		}

		[Theory]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData("")]
		[InlineData("abcd")]
		public void ReturnsCorrectResult(object value)
		{
			var apiAttribute = new ApiRequiredAttribute();
			var originalAttribute = new RequiredAttribute();

			AssertSameResult(value, apiAttribute, originalAttribute);
		}
	}
}