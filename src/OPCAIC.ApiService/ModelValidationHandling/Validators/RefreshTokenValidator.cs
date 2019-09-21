using FluentValidation;
using OPCAIC.ApiService.Models;
using OPCAIC.Application.Infrastructure.Validation;

namespace OPCAIC.ApiService.ModelValidationHandling.Validators
{
	public class RefreshTokenValidator : AbstractValidator<RefreshToken>
	{
		public RefreshTokenValidator()
		{
			RuleFor(m => m.Token).MinLength(1).Required();
		}
	}
}