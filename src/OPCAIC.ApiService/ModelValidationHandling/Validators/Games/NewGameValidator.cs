using FluentValidation;
using OPCAIC.ApiService.Models.Games;
using OPCAIC.Infrastructure;

namespace OPCAIC.ApiService.ModelValidationHandling.Validators.Games
{
	public class NewGameValidator : AbstractValidator<NewGameModel>
	{
		public NewGameValidator()
		{
			RuleFor(m => m.Name).Required().MaxLength(StringLengths.GameName);
			RuleFor(m => m.Key).Required().MaxLength(StringLengths.GameKey);
			RuleFor(m => m.ConfigurationSchema).Required().ValidSchema();
			RuleFor(m => m.MaxAdditionalFilesSize).MinValue(1);
		}
	}
}