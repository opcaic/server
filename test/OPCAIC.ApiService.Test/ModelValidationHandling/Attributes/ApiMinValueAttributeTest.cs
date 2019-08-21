using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Moq;
using OPCAIC.ApiService.ModelValidationHandling;
using OPCAIC.ApiService.ModelValidationHandling.Attributes;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.ApiService.Test.ModelValidationHandling.Attributes
{
	public class ApiMinValueAttributeTest : ApiAttributeTestBase
	{
		/// <inheritdoc />
		public ApiMinValueAttributeTest(ITestOutputHelper output) : base(output)
		{
			// Prepare validation result that is returned by IModelValidationService
			var validationResultId = Guid.NewGuid().ToString();
			var expectedValidationResult = new ValidationResult(validationResultId);

			// Setup IModelValidationService.ProcessValidationError()
			ModelValidationService
				.Setup(x => x.ProcessValidationError(It.IsAny<IEnumerable<string>>(),
					It.IsAny<ValidationErrorBase>())).Returns(expectedValidationResult)
				.Verifiable();
		}

		[Theory]
		[InlineData(1, 3, false)]
		[InlineData(1, 1, false)]
		public void ReturnsCorrectResultSuccess(int minvalue, int value, bool strict)
		{
			var apiAttribute = new ApiMinValueAttribute(minvalue) {IsStrict = strict};
			var validationContext = new ValidationContext(this, ServiceProvider, null);

			var result = apiAttribute.GetValidationResult(value, validationContext);

			Assert.Equal(ValidationResult.Success, result);
		}

		[Theory]
		[InlineData(1, 0, false)]
		[InlineData(1, 1, true)]
		public void ReturnsCorrectResultFailure(int minvalue, int value, bool strict)
		{
			var apiAttribute = new ApiMinValueAttribute(minvalue) {IsStrict = strict};
			var validationContext = new ValidationContext(this, ServiceProvider, null);

			var result = apiAttribute.GetValidationResult(value, validationContext);

			Assert.NotEqual(ValidationResult.Success, result);
		}
	}
}