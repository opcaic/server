using System.ComponentModel.DataAnnotations;
using OPCAIC.ApiService.ModelValidationHandling.Attributes;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.ApiService.Test.ModelValidationHandling.Attributes
{
	public class ApiMaxLengthAttributeTest : ApiAttributeTestBase
	{
		/// <inheritdoc />
		public ApiMaxLengthAttributeTest(ITestOutputHelper output) : base(output)
		{
		}

		[Theory]
		[InlineData("abcd", 3)]
		[InlineData("abcd", 4)]
		[InlineData("abcd", 5)]
		[InlineData("", 1)]
		public void ReturnsCorrectResult(string value, int maxLength)
		{
			var apiAttribute = new ApiMaxLengthAttribute(maxLength);
			var originalAttribute = new MaxLengthAttribute(maxLength);

			AssertSameResult(value, apiAttribute, originalAttribute);
		}
	}
}