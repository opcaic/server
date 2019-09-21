using FluentValidation;
using OPCAIC.ApiService.Models.Users;
using OPCAIC.Application.Infrastructure.Validation;

namespace OPCAIC.ApiService.ModelValidationHandling.Validators.Users
{
	public class NewPasswordValidator : AbstractValidator<NewPasswordModel>
	{
		public NewPasswordValidator()
		{
			RuleFor(m => m.OldPassword).Required();

			RuleFor(m => m.NewPassword).Required();
		}
	}
}