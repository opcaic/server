using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using OPCAIC.Common;
using OPCAIC.Domain.Entities;
using OPCAIC.Utils;

namespace OPCAIC.ApiService.Extensions
{
	internal static class UserLoggingExtensions
	{
		public static void LoginSuccess(this ILogger logger, User user)
		{
			logger.LogInformation(LoggingEvents.LoginSuccess, $"User {{{LoggingTags.UserId}}} logged in.", user.Id);
		}

		public static void LoginNotAllowed(this ILogger logger, User user)
		{
			logger.LogInformation(LoggingEvents.LoginNotAllowed, $"User {{{LoggingTags.UserId}}} is not allowed to log in.", user.Id);
		}

		public static void LoginLockout(this ILogger logger, User user)
		{
			logger.LogInformation(LoggingEvents.LoginLockout, $"User {{{LoggingTags.UserId}}} is locked out.", user.Id);
		}

		public static void LoginInvalidPassword(this ILogger logger, User user)
		{
			logger.LogInformation(LoggingEvents.LoginInvalidPassword, $"Invalid password for user {{{LoggingTags.UserId}}}.", user.Id);
		}

		public static void LoginInvalidMail(this ILogger logger, string email)
		{
			logger.LogInformation(LoggingEvents.LoginInvalidEmail, $"Invalid login email {{{LoggingTags.UserEmail}}}.", email);
		}

		public static void UserCreated(this ILogger logger, User user)
		{
			logger.LogInformation(LoggingEvents.UserCreated, $"User {{{LoggingTags.UserEmail}}} created with id {{{LoggingTags.UserId}}}", user.Email, user.Id);
		}

		public static void UserForgotPassword(this ILogger logger, User user)
		{
			logger.LogInformation(LoggingEvents.UserForgotPassword, $"Sending password reset email to {{{LoggingTags.UserEmail}}} for user {{{LoggingTags.UserId}}}", user.Email, user.Id);
		}

		public static void UserPasswordReset(this ILogger logger, User user, IdentityResult result)
		{
			logger.LogInformation(LoggingEvents.UserPasswordReset, $"Resetting password for user {{{LoggingTags.UserId}}} with result {{{LoggingTags.IdentityResult}}}", user.Id, result);
		}

		public static void UserPasswordChange(this ILogger logger, User user, IdentityResult result)
		{
			logger.LogInformation(LoggingEvents.UserPasswordChange, $"Changing password for user {{{LoggingTags.UserId}}} with result {{{LoggingTags.IdentityResult}}}", user.Id, result);
		}

		public static void UserConfirmEmail(this ILogger logger, User user, IdentityResult result)
		{
			logger.LogInformation(LoggingEvents.UserEmailConfirm, $"Confirming email for user {{{LoggingTags.UserId}}} with result {{{LoggingTags.IdentityResult}}}", user.Id, result);
		}

		public static void UserVerificationResend(this ILogger logger, User user)
		{
			logger.LogInformation(LoggingEvents.UserEmailConfirmResend, $"Resending verification email for user {{{LoggingTags.UserId}}} to {{{LoggingTags.UserEmail}}}.", user.Id, user.Email);
		}
	}
}