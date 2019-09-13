using FluentValidation;
using OPCAIC.ApiService.Models.Tournaments;

namespace OPCAIC.ApiService.ModelValidationHandling.Validators.Tournaments
{
	public class DeleteTournamentParticipantValidator : AbstractValidator<DeleteTournamentParticipantModel>
	{
		public DeleteTournamentParticipantValidator()
		{
			RuleFor(m => m.Email).Required().Email();
		}
	}
}