using System.ComponentModel.DataAnnotations;
using OPCAIC.ApiService.ModelValidationHandling;
using OPCAIC.Application.Infrastructure.Validation;
using Xunit;

namespace OPCAIC.ApiService.Test.ModelValidationHandling
{
	public class ModelValidationServiceTest
	{
		private class TestValidationError : ValidationError
		{
			/// <inheritdoc />
			public TestValidationError(string code, string message) : base(code, message, null)
			{
			}
		}

		[Fact]
		public void ErrorsCanBeRetrieved()
		{
			var modelValidationService = new ModelValidationService();

			var errorCode = "code";
			var errorMessage = "message";
			var error = new TestValidationError(errorCode, errorMessage);

			var validationResult = new ValidationResult(errorMessage);

			var returnedValidationResult =
				modelValidationService.ProcessValidationError(validationResult.MemberNames, error);

			Assert.NotNull(returnedValidationResult);
			Assert.NotNull(returnedValidationResult.ErrorMessage);

			var id = returnedValidationResult.ErrorMessage;

			var returnedError = modelValidationService.GetValidationError(id);

			Assert.NotNull(returnedError);
			Assert.Equal(errorCode, returnedError.Code);
			Assert.Equal(errorMessage, returnedError.Message);
		}
	}
}