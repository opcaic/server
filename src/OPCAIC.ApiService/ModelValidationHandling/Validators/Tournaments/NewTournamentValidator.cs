using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Validators;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OPCAIC.ApiService.Models.Submissions;
using OPCAIC.ApiService.Models.Tournaments;
using OPCAIC.Infrastructure;
using OPCAIC.Infrastructure.DbContexts;
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

			RuleFor(m => m.MaxSubmissionSize).MinValue(1);
		}
	}

	public class AdditionalTournamentFilesValidator
		: AbstractValidator<AdditionalTournamentFilesModel>
	{
		public AdditionalTournamentFilesValidator()
		{
			RuleFor(m => m.Archive).CustomAsync(Action);
		}

		private async Task Action(IFormFile archive, CustomContext context, CancellationToken cancellationToken)
		{
			var id = ((AdditionalTournamentFilesModel)context.InstanceToValidate).TournamentId;
			var dbContext = context.GetServiceProvider().GetRequiredService<DataContext>();

			// check archive size
			var game = await dbContext.Tournaments.Where(t => t.Id == id)
				.Select(s => new {s.Game.MaxAdditionalFilesSize}).SingleOrDefaultAsync(cancellationToken);

			if (game == null || archive == null)
				return; // nothing to validate

			ArchiveValidationHelper.ValidateArchive(archive, context,
				game.MaxAdditionalFilesSize);
		}
	}
}