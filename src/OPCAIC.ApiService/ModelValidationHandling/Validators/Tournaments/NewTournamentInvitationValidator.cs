using FluentValidation;
using OPCAIC.ApiService.Models.Tournaments;
using OPCAIC.Domain.Entities;

namespace OPCAIC.ApiService.ModelValidationHandling.Validators.Tournaments
{
	public class NewTournamentInvitationValidator
		: AbstractValidator<NewTournamentInvitationsModel>
	{
		public NewTournamentInvitationValidator()
		{
			RuleFor(m => m.TournamentId).EntityId(typeof(Tournament));
			RuleFor(m => m.Emails).ForEach(f => f.Email());
		}
	}
}