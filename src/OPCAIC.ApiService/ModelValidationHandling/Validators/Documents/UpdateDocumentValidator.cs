using FluentValidation;
using OPCAIC.ApiService.Models.Documents;
using OPCAIC.Application.Infrastructure.Validation;
using OPCAIC.Common;
using OPCAIC.Domain.Entities;
using OPCAIC.Infrastructure;

namespace OPCAIC.ApiService.ModelValidationHandling.Validators.Documents
{
	public class UpdateDocumentValidator : AbstractValidator<UpdateDocumentModel>
	{
		public UpdateDocumentValidator()
		{
			RuleFor(m => m.Name).MaxLength(StringLengths.DocumentName).Required();
			RuleFor(m => m.TournamentId).EntityId(typeof(Tournament));
		}
	}
}