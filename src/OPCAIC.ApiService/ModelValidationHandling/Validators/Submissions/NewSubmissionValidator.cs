using System;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using FluentValidation.Validators;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OPCAIC.ApiService.Models.Submissions;
using OPCAIC.Domain.Entities;
using OPCAIC.Persistence;

namespace OPCAIC.ApiService.ModelValidationHandling.Validators.Submissions
{
	public class NewSubmissionValidator : AbstractValidator<NewSubmissionModel>
	{
		public NewSubmissionValidator()
		{
			RuleFor(m => m.TournamentId).EntityId(typeof(Tournament));
			RuleFor(m => m.Archive).Required()
				.CustomAsync(ArchiveValidation);
		}

		private async Task ArchiveValidation(IFormFile archive, CustomContext context, CancellationToken cancellationToken)
		{
			var id = ((NewSubmissionModel)context.InstanceToValidate).TournamentId;
			var dbContext = context.GetServiceProvider().GetRequiredService<DataContext>();

			// check archive size
			var tournament = await dbContext.Tournaments.Where(t => t.Id == id)
				.Select(s => new {s.MaxSubmissionSize}).SingleOrDefaultAsync(cancellationToken);

			if (tournament == null || archive == null)
				return; // nothing to validate

			ArchiveValidationHelper.ValidateArchive(archive, context,
				tournament.MaxSubmissionSize);
		}
	}
}