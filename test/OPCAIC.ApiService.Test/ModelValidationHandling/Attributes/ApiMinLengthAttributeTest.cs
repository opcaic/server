using System.ComponentModel.DataAnnotations;
using OPCAIC.ApiService.ModelValidationHandling.Attributes;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.ApiService.Test.ModelValidationHandling.Attributes
{
	public class ApiMinLengthAttributeTest : ApiAttributeTestBase
	{
		/// <inheritdoc />
		public ApiMinLengthAttributeTest(ITestOutputHelper output) : base(output)
		{
		}

		[Theory]
		[InlineData("abcd", 3)]
		[InlineData("abcd", 4)]
		[InlineData("abcd", 5)]
		[InlineData("", 1)]
		public void ReturnsCorrectResult(string value, int minLength)
		{
			var apiAttribute = new ApiMinLengthAttribute(minLength);
			var originalAttribute = new MinLengthAttribute(minLength);

			AssertSameResult(value, apiAttribute, originalAttribute);
		}
	}
}