﻿using FluentValidation;
using OPCAIC.ApiService.Models.Users;

namespace OPCAIC.ApiService.ModelValidationHandling.Validators.Users
{
	public class PasswordResetValidator : AbstractValidator<PasswordResetModel>
	{
		public PasswordResetValidator()
		{
			RuleFor(m => m.Email).Email().Required();
			RuleFor(m => m.ResetToken).Required();
			RuleFor(m => m.Password).Required();
		}
	}
}