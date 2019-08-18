﻿using Microsoft.AspNetCore.Identity;
using OPCAIC.ApiService.Models.Users;
using OPCAIC.ApiService.ModelValidationHandling;
using OPCAIC.Infrastructure.Entities;

namespace OPCAIC.ApiService.Security
{
	public class AppIdentityErrorDescriber : IdentityErrorDescriber
	{
		private AppIdentityError Map(IdentityError baseError, ValidationErrorBase validationError)
		{
			validationError.Message = baseError.Description;
			return new AppIdentityError
			{
				Description = baseError.Description,
				Code = validationError.Code,
				ValidationError = validationError
			};
		}

		// TODO: override with custom error codes?

		/// <inheritdoc />
		public override IdentityError DefaultError()
		{
			return base.DefaultError();
		}

		/// <inheritdoc />
		public override IdentityError ConcurrencyFailure()
		{
			return base.ConcurrencyFailure();
		}

		/// <inheritdoc />
		public override IdentityError PasswordMismatch()
		{
			return Map(base.PasswordMismatch(), new IdentityValidationError(ValidationErrorCodes.PasswordMismatch));
		}

		/// <inheritdoc />
		public override IdentityError InvalidToken()
		{
			// TODO: can this be called for non-email token in our code?
			return Map(base.InvalidToken(), new IdentityValidationError(ValidationErrorCodes.InvalidEmailVerificationToken));
		}

		/// <inheritdoc />
		public override IdentityError RecoveryCodeRedemptionFailed()
		{
			return base.RecoveryCodeRedemptionFailed();
		}

		/// <inheritdoc />
		public override IdentityError LoginAlreadyAssociated()
		{
			return base.LoginAlreadyAssociated();
		}

		/// <inheritdoc />
		public override IdentityError InvalidUserName(string userName)
		{
			return Map(base.InvalidUserName(userName),
				new UserNameValidationError(ValidationErrorCodes.InvalidUsernameError, userName));
		}

		/// <inheritdoc />
		public override IdentityError InvalidEmail(string email)
		{
			return Map(base.InvalidEmail(email),
				new EmailValidationError(ValidationErrorCodes.InvalidEmailError, email));
		}

		/// <inheritdoc />
		public override IdentityError DuplicateUserName(string userName)
		{
			return Map(base.DuplicateUserName(userName),
				new UserNameValidationError(ValidationErrorCodes.UserUsernameConflict, userName));
		}

		/// <inheritdoc />
		public override IdentityError DuplicateEmail(string email)
		{
			return Map(base.DuplicateEmail(email),
				new EmailValidationError(ValidationErrorCodes.UserEmailConflict, email));
		}

		/// <inheritdoc />
		public override IdentityError InvalidRoleName(string role)
		{
			return base.InvalidRoleName(role);
		}

		/// <inheritdoc />
		public override IdentityError DuplicateRoleName(string role)
		{
			return base.DuplicateRoleName(role);
		}

		/// <inheritdoc />
		public override IdentityError UserAlreadyHasPassword()
		{
			return base.UserAlreadyHasPassword();
		}

		/// <inheritdoc />
		public override IdentityError UserLockoutNotEnabled()
		{
			return base.UserLockoutNotEnabled();
		}

		/// <inheritdoc />
		public override IdentityError UserAlreadyInRole(string role)
		{
			return base.UserAlreadyInRole(role);
		}

		/// <inheritdoc />
		public override IdentityError UserNotInRole(string role)
		{
			return base.UserNotInRole(role);
		}

		/// <inheritdoc />
		public override IdentityError PasswordTooShort(int length)
		{
			return Map(base.PasswordTooShort(length), new PasswordLengthError(length));
		}

		/// <inheritdoc />
		public override IdentityError PasswordRequiresUniqueChars(int uniqueChars)
		{
			return Map(base.PasswordRequiresUniqueChars(uniqueChars),
				new PasswordUniqueCharsError(uniqueChars));
		}

		/// <inheritdoc />
		public override IdentityError PasswordRequiresNonAlphanumeric()
		{
			return Map(base.PasswordRequiresNonAlphanumeric(),
				new IdentityValidationError(ValidationErrorCodes.PasswordRequiresNonAlphanumeric));
		}

		/// <inheritdoc />
		public override IdentityError PasswordRequiresDigit()
		{
			return Map(base.PasswordRequiresDigit(),
				new IdentityValidationError(ValidationErrorCodes.PasswordRequiresDigit));
		}

		/// <inheritdoc />
		public override IdentityError PasswordRequiresLower()
		{
			return Map(base.PasswordRequiresLower(),
				new IdentityValidationError(ValidationErrorCodes.PasswordRequiresLower));
		}

		/// <inheritdoc />
		public override IdentityError PasswordRequiresUpper()
		{
			return Map(base.PasswordRequiresUpper(),
				new IdentityValidationError(ValidationErrorCodes.PasswordRequiresUpper));
		}

		public class IdentityValidationError : ValidationErrorBase
		{
			/// <inheritdoc />
			public IdentityValidationError(string code)
				: base(code, "") // message will be replaced by IdentityError.Description
			{
			}
		}

		public class UserNameValidationError : IdentityValidationError
		{
			/// <inheritdoc />
			public UserNameValidationError(string code, string username)
				: base(code)
			{
				Username = username;
				Field = nameof(Username);
			}

			public string Username { get; set; }
		}

		public class EmailValidationError : IdentityValidationError
		{
			public EmailValidationError(string code, string email)
				: base(code)
			{
				Email = email;
				Field = nameof(Email);
			}

			public string Email { get; set; }
		}

		public class PasswordLengthError : IdentityValidationError
		{
			public PasswordLengthError(int length)
				: base(ValidationErrorCodes.PasswordTooShort)
			{
				Minimum = length;
				Field = nameof(UserCredentialsModel.Password);
			}

			public int Minimum { get; set; }
		}

		public class PasswordUniqueCharsError : IdentityValidationError
		{
			public PasswordUniqueCharsError(int count)
				: base(ValidationErrorCodes.PasswordRequiresUnique)
			{
				Count = count;
				Field = nameof(UserCredentialsModel.Password);
			}

			public int Count { get; set; }
		}
	}
}