using FluentValidation;
using OPCAIC.ApiService.Models.Users;
using OPCAIC.Application.Infrastructure.Validation;

namespace OPCAIC.ApiService.ModelValidationHandling.Validators.Users
{
	public class NewUserValidator : AbstractValidator<NewUserModel>
	{
		public NewUserValidator()
		{
			RuleFor(m => m.Email).Email().Required();
			RuleFor(m => m.Username).Required();
			RuleFor(m => m.Organization).MinLength(1);

			// TODO: choice from available localizations
			RuleFor(m => m.LocalizationLanguage).Required().MinLength(2).MaxLength(2);

			RuleFor(m => m.Password).Required();
		}
	}
}