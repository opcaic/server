using FluentValidation;
using OPCAIC.ApiService.Models.Games;
using OPCAIC.Infrastructure;

namespace OPCAIC.ApiService.ModelValidationHandling.Validators.Games
{
	public class UpdateGameValidator : AbstractValidator<UpdateGameModel>
	{
		public UpdateGameValidator()
		{
			RuleFor(m => m.Name).Required().MaxLength(StringLengths.GameName);
			RuleFor(m => m.Key).Required().MaxLength(StringLengths.GameKey);
			RuleFor(m => m.ConfigurationSchema).Required().ValidSchema();
		}
	}
}