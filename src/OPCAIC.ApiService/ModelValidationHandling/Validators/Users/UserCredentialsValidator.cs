using FluentValidation;
using OPCAIC.ApiService.Models.Users;

namespace OPCAIC.ApiService.ModelValidationHandling.Validators.Users
{
	public class UserCredentialsValidator : AbstractValidator<UserCredentialsModel>
	{
		public UserCredentialsValidator()
		{
			RuleFor(m => m.Email).Email().Required();
			RuleFor(m => m.Password).Required();
		}
	}
}