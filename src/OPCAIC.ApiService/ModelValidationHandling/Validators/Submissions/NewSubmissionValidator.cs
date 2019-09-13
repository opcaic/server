using FluentValidation;
using OPCAIC.ApiService.Models.Submissions;
using OPCAIC.Infrastructure.Entities;

namespace OPCAIC.ApiService.ModelValidationHandling.Validators.Submissions
{
	public class NewSubmissionValidator : AbstractValidator<NewSubmissionModel>
	{
		public NewSubmissionValidator()
		{
			RuleFor(m => m.TournamentId).EntityId(typeof(Tournament));
			RuleFor(m => m.Archive).Required();
		}
	}
}