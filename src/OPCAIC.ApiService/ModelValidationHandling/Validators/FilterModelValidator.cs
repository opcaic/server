using FluentValidation;
using OPCAIC.ApiService.Models;
using OPCAIC.ApiService.Models.Documents;
using OPCAIC.ApiService.Models.Games;
using OPCAIC.ApiService.Models.Matches;
using OPCAIC.ApiService.Models.Submissions;
using OPCAIC.ApiService.Models.Tournaments;
using OPCAIC.ApiService.Models.Users;
using OPCAIC.Application.Infrastructure.Validation;

namespace OPCAIC.ApiService.ModelValidationHandling.Validators
{
	// TODO: gradually move these to Application
	public abstract class FilterModelValidator<T> : AbstractValidator<T>
		where T : FilterModelBase
	{
		protected FilterModelValidator()
		{
			RuleFor(m => m.Offset).MinValue(0);
			RuleFor(m => m.Count).InRange(1, 100);
			RuleFor(m => m.SortBy).MinLength(1);
		}
	}

	public class DocumentFilterValidator : FilterModelValidator<DocumentFilterModel>
	{
		public DocumentFilterValidator()
		{
			RuleFor(m => m.Name).MinLength(1);
		}
	}

	public class MatchFilterValidator : FilterModelValidator<MatchFilterModel>
	{
	}

	public class SubmissionFilterValidator : FilterModelValidator<SubmissionFilterModel>
	{
	}

	public class TournamentInvitationFilterValidator
		: FilterModelValidator<TournamentInvitationFilter>
	{
	}

	public class UserFilterValidator : FilterModelValidator<UserFilterModel>
	{
		public UserFilterValidator()
		{
			RuleFor(m => m.Email).MinLength(1);
			RuleFor(m => m.Username).MinLength(1);
			RuleFor(m => m.UserRole).IsInEnum();
		}
	}
}