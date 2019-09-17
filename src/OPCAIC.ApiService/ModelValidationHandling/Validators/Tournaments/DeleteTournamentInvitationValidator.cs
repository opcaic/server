using FluentValidation;
using OPCAIC.ApiService.Models.Tournaments;

namespace OPCAIC.ApiService.ModelValidationHandling.Validators.Tournaments
{
	public class DeleteTournamentInvitationValidator : AbstractValidator<DeleteTournamentInvitationModel>
	{
		public DeleteTournamentInvitationValidator()
		{
			RuleFor(m => m.Email).Required().Email();
		}
	}
}