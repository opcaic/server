using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Validators;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OPCAIC.ApiService.Models.Tournaments;
using OPCAIC.Persistence;

namespace OPCAIC.ApiService.ModelValidationHandling.Validators.Tournaments
{
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