using System.ComponentModel.DataAnnotations;
using OPCAIC.ApiService.ModelValidationHandling.Attributes;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.ApiService.Test.ModelValidationHandling.Attributes
{
	public class ApiEmailAddressAttributeTest : ApiAttributeTestBase
	{
		/// <inheritdoc />
		public ApiEmailAddressAttributeTest(ITestOutputHelper output) : base(output)
		{
		}

		[Theory]
		[InlineData("abcd")]
		[InlineData("abcd@ef")]
		[InlineData("abcd@ef.gh")]
		public void ReturnsCorrectResult(string value)
		{
			var apiAttribute = new ApiEmailAddressAttribute();
			var originalAttribute = new EmailAddressAttribute();

			AssertSameResult(value, apiAttribute, originalAttribute);
		}
	}
}