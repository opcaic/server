using FluentValidation;
using OPCAIC.ApiService.Models.Tournaments;
using OPCAIC.Application.Infrastructure.Validation;
using OPCAIC.Domain.Entities;

namespace OPCAIC.ApiService.ModelValidationHandling.Validators.Tournaments
{
	public class NewTournamentInvitationValidator
		: AbstractValidator<NewTournamentInvitationsModel>
	{
		public NewTournamentInvitationValidator()
		{
			RuleFor(m => m.Emails).ForEach(f => f.Email());
		}
	}
}