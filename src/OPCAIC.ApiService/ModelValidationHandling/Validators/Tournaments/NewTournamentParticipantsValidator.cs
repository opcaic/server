using FluentValidation;
using OPCAIC.ApiService.Models.Tournaments;
using OPCAIC.Infrastructure.Entities;

namespace OPCAIC.ApiService.ModelValidationHandling.Validators.Tournaments
{
	public class NewTournamentParticipantsValidator
		: AbstractValidator<NewTournamentParticipantsModel>
	{
		public NewTournamentParticipantsValidator()
		{
			RuleFor(m => m.TournamentId).EntityId(typeof(Tournament));
			RuleFor(m => m.Emails).ForEach(f => f.Email());
		}
	}
}