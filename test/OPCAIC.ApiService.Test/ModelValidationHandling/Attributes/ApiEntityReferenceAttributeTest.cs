using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Moq;
using OPCAIC.ApiService.ModelValidationHandling;
using OPCAIC.ApiService.ModelValidationHandling.Attributes;
using OPCAIC.Infrastructure.DbContexts;
using OPCAIC.Infrastructure.Entities;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.ApiService.Test.ModelValidationHandling.Attributes
{
	public class ApiEntityReferenceAttributeTest : ApiAttributeTestBase
	{
		private const long ValidId = 1;
		/// <inheritdoc />
		public ApiEntityReferenceAttributeTest(ITestOutputHelper output) : base(output)
		{
			UseDatabase();

			// Prepare validation result that is returned by IModelValidationService
			var validationResultId = Guid.NewGuid().ToString();
			var expectedValidationResult = new ValidationResult(validationResultId);

			// Setup IModelValidationService.ProcessValidationError()
			ModelValidationService
				.Setup(x => x.ProcessValidationError(It.IsAny<IEnumerable<string>>(),
					It.IsAny<ValidationErrorBase>())).Returns(expectedValidationResult)
				.Verifiable();

			// prepare test entity to be referenced
			var context = GetService<DataContext>();
			var game = new Game()
			{
				Id = ValidId,
				Name = "Test game"
			};
			context.Games.Add(game);
			context.SaveChanges();
		}

		private class GameReference
		{
			[ApiEntityReference(typeof(Game))]
			public long GameId { get; set; }
		}

		[Fact]
		public void ReturnsCorrectResult_Success()
		{
			var model = new GameReference { GameId = ValidId };
			var validationContext = new ValidationContext(model, ServiceProvider, null);
			var attribute = new ApiEntityReferenceAttribute(typeof(Game));

			// no throw
			attribute.Validate(model.GameId, validationContext);
		}

		[Fact]
		public void ReturnsCorrectResult_Fail()
		{
			var model = new GameReference { GameId = ValidId + 1 }; // oops
			var validationContext = new ValidationContext(model, ServiceProvider, null);
			var attribute = new ApiEntityReferenceAttribute(typeof(Game));

			Assert.Throws<ValidationException>(() => attribute.Validate(model.GameId, validationContext));
		}
	}
}