﻿using FluentValidation;
using OPCAIC.ApiService.Models.Users;
using OPCAIC.Application.Infrastructure.Validation;

namespace OPCAIC.ApiService.ModelValidationHandling.Validators.Users
{
	public class ForgotPasswordValidator : AbstractValidator<ForgotPasswordModel>
	{
		public ForgotPasswordValidator()
		{
			RuleFor(m => m.Email).Email().Required();
		}
	}
}