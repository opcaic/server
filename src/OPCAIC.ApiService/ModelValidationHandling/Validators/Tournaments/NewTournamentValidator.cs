using FluentValidation;
using OPCAIC.ApiService.Models.Tournaments;
using OPCAIC.Infrastructure;
using OPCAIC.Infrastructure.Entities;
using OPCAIC.Infrastructure.Enums;

namespace OPCAIC.ApiService.ModelValidationHandling.Validators.Tournaments
{
	public class NewTournamentValidator : AbstractValidator<NewTournamentModel>
	{
		public NewTournamentValidator()
		{
			RuleFor(m => m.Name).Required().MaxLength(StringLengths.TournamentName);
			RuleFor(m => m.GameId).EntityId(typeof(Game));

			// keep these rules synchronized with UpdateTournamentModel
			RuleFor(m => m.Format).IsInEnum().NotEqual(TournamentFormat.Unknown)
				.NotEqual(TournamentFormat.Elo).When(m => m.Scope == TournamentScope.Deadline)
				.WithMessage("Deadline tournaments do not support ELO format.")
				.Equal(TournamentFormat.Elo).When(m => m.Scope == TournamentScope.Ongoing)
				.WithMessage("Only ELO format is supported for ongoing tournaments");

			RuleFor(m => m.Scope).IsInEnum().NotEqual(TournamentScope.Unknown);
			RuleFor(m => m.RankingStrategy).IsInEnum().NotEqual(TournamentRankingStrategy.Unknown);

			RuleFor(m => m.MatchesPerDay)
				.Null().When(m => m.Scope == TournamentScope.Deadline)
				.NotNull().When(m => m.Scope == TournamentScope.Ongoing);
		}
	}
}