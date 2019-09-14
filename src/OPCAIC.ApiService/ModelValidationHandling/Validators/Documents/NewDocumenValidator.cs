using FluentValidation;
using OPCAIC.ApiService.Models.Documents;
using OPCAIC.Infrastructure;
using OPCAIC.Infrastructure.Entities;

namespace OPCAIC.ApiService.ModelValidationHandling.Validators.Documents
{
	public class NewDocumenValidator : AbstractValidator<NewDocumentModel>
	{
		public NewDocumenValidator()
		{
			RuleFor(m => m.Name).MaxLength(StringLengths.DocumentName).Required();
			RuleFor(m => m.TournamentId).EntityId(typeof(Tournament));
		}
	}
}