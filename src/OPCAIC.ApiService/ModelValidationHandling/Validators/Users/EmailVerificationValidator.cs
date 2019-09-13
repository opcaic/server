using FluentValidation;
using OPCAIC.ApiService.Models.Users;

namespace OPCAIC.ApiService.ModelValidationHandling.Validators.Users
{
	public class EmailVerificationValidator : AbstractValidator<EmailVerificationModel>
	{
		public EmailVerificationValidator()
		{
			RuleFor(m => m.Email).Email().Required();
			RuleFor(m => m.Token).Required();
		}
	}
}