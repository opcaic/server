using System;
using System.ComponentModel.DataAnnotations;
using Moq;
using OPCAIC.ApiService.ModelValidationHandling;
using OPCAIC.TestUtils;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.ApiService.Test.ModelValidationHandling.Attributes
{
	public class ApiAttributeTestBase : ServiceTestBase
	{
		private readonly Mock<IModelValidationService> modelValidationService;

		/// <inheritdoc />
		public ApiAttributeTestBase(ITestOutputHelper output) : base(output)
		{
			modelValidationService = Services.Mock<IModelValidationService>();
		}

		protected void AssertSameResult<TValue>(TValue value, ValidationAttribute apiAttribute,
			ValidationAttribute originalAttribute)
		{
			// Prepare validation result that is returned by IModelValidationService
			var validationResultId = Guid.NewGuid().ToString();
			var expectedValidationResult = new ValidationResult(validationResultId);

			// Setup IModelValidationService.ProcessValidationError()
			modelValidationService
				.Setup(x => x.ProcessValidationError(It.IsAny<ValidationResult>(),
					It.IsAny<ValidationErrorBase>())).Returns(expectedValidationResult)
				.Verifiable();

			var validationContext = new ValidationContext(value, ServiceProvider, null);

			// Get validation results from both attributes
			var result = apiAttribute.GetValidationResult(value, validationContext);
			var originalResult = originalAttribute.GetValidationResult(value, validationContext);

			// Check that the validation result is either successful or equal to the prepared one
			if (originalResult == ValidationResult.Success)
			{
				Assert.Equal(ValidationResult.Success, result);
			}
			else
			{
				Assert.Equal(expectedValidationResult, result);

				modelValidationService.Verify(
					x => x.ProcessValidationError(It.IsAny<ValidationResult>(),
						It.IsAny<ValidationErrorBase>()), Times.Once);
			}
		}
	}
}