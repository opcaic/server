using FluentValidation;
using OPCAIC.ApiService.Models;

namespace OPCAIC.ApiService.ModelValidationHandling.Validators
{
	public class ResultArchiveValidator : AbstractValidator<ResultArchiveModel>
	{
		public ResultArchiveValidator()
		{
			RuleFor(m => m.Archive).Required().Custom(ArchiveValidationHelper.ValidateArchive);
		}
	}
}