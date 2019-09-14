using FluentValidation;
using OPCAIC.ApiService.Models;

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