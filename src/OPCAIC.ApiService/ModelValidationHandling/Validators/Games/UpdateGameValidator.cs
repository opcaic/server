using FluentValidation;
using OPCAIC.ApiService.Models.Games;
using OPCAIC.Application.Infrastructure.Validation;
using OPCAIC.Common;

namespace OPCAIC.ApiService.ModelValidationHandling.Validators.Games
{
	public class UpdateGameValidator : AbstractValidator<UpdateGameModel>
	{
		public UpdateGameValidator()
		{
			RuleFor(m => m.Name).Required().MaxLength(StringLengths.GameName);
			RuleFor(m => m.Key).Required().MaxLength(StringLengths.GameKey);
			RuleFor(m => m.DefaultTournamentImageUrl).MaxLength(StringLengths.GameDefaultTournamentImageUrl);
			RuleFor(m => m.ImageUrl).MaxLength(StringLengths.GameImageUrl);
			RuleFor(m => m.Description).MaxLength(StringLengths.GameDescription);
			RuleFor(m => m.DefaultTournamentImageOverlay).InclusiveBetween(0, 1);
			RuleFor(m => m.ConfigurationSchema).ValidSchema();
			RuleFor(m => m.MaxAdditionalFilesSize).MinValue(1);
		}
	}
}