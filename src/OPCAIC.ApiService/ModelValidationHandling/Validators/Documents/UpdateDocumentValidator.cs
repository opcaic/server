using FluentValidation;
using OPCAIC.ApiService.Models.Documents;
using OPCAIC.Infrastructure;
using OPCAIC.Infrastructure.Entities;

namespace OPCAIC.ApiService.ModelValidationHandling.Validators.Documents
{
	public class UpdateDocumentValidator : AbstractValidator<UpdateDocumentModel>
	{
		public UpdateDocumentValidator()
		{
			RuleFor(m => m.Name).MinLength(StringLengths.DocumentName).Required();
			RuleFor(m => m.TournamentId).EntityId(typeof(Tournament));
		}
	}
}