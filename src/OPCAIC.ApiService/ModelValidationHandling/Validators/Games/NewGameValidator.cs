using FluentValidation;
using OPCAIC.ApiService.Models.Games;
using OPCAIC.Application.Infrastructure.Validation;
using OPCAIC.Common;
using OPCAIC.Infrastructure;

namespace OPCAIC.ApiService.ModelValidationHandling.Validators.Games
{
	public class NewGameValidator : AbstractValidator<NewGameModel>
	{
		public NewGameValidator()
		{
			RuleFor(m => m.Name).Required().MaxLength(StringLengths.GameName);
			RuleFor(m => m.Key).Required().MaxLength(StringLengths.GameKey);
			RuleFor(m => m.DefaultTournamentImageUrl).Url().MaxLength(StringLengths.GameDefaultTournamentImageUrl);
			RuleFor(m => m.ImageUrl).Url().MaxLength(StringLengths.GameImageUrl);
			RuleFor(m => m.Description).MaxLength(StringLengths.GameDescription);
			RuleFor(m => m.DefaultTournamentImageOverlay).InRange(0, 1);
			RuleFor(m => m.MaxAdditionalFilesSize).MinValue(1);
		}
	}
}