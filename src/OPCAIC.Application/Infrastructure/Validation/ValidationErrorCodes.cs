﻿namespace OPCAIC.Application.Infrastructure.Validation
{
	public static class ValidationErrorCodes
	{
		#region Emails

		public static string InvalidEmailVerificationToken => "invalid-email-verification-token";

		#endregion

		#region General errors

		public static string GenericError => "generic-error";

		public static string MinLengthError => "field-min-length";

		public static string MaxLengthError => "field-max-length";

		public static string StringLengthError => "field-string-length";

		public static string MinValueError => "field-min-value";

		public static string RequiredError => "field-required";

		public static string InvalidEmailError => "field-invalid-email";

		public static string InvalidUrlError => "field-invalid-url";

		public static string InvalidUsernameError => "field-invalid-username";

		public static string RangeError => "field-range-error";

		public static string TournamentDeadlinePassed => "deadline-passed";

		public static string TournamentDoesNotAcceptSubmission => "tournament-not-active";

		public static string TournamentEvaluationAlreadyStarted => "evaluation-already-started";

		#endregion

		#region Password

		public static string PasswordTooShort => "field-password-short";

		public static string PasswordRequiresUnique => "field-password-unique-chars";

		public static string PasswordRequiresNonAlphanumeric => "field-password-unique-chars";

		public static string PasswordRequiresDigit => "field-password-digit";

		public static string PasswordRequiresLower => "field-password-lower";

		public static string PasswordRequiresUpper => "field-password-upper";

		public static string PasswordMismatch => "field-password-mismatch";

		#endregion

		#region Conflicts

		public static string GameNameConflict => "game-name-conflict";

		public static string UserEmailConflict => "user-email-conflict";

		public static string UserUsernameConflict => "user-username-conflict";

		#endregion

		#region Unauthorized user

		public static string LoginEmailNotConfirmed => "login-email-not-confirmed";

		public static string LoginLockout => "login-lockout";

		public static string LoginInvalid => "login-invalid";

		public static string RefreshTokenInvalid => "invalid-token";

		public static string UserNotInvited => "user-not-tounament-participant";

		#endregion

		public static string UploadNotZip => "not-zip-file";

		public static string InvalidReference => "invalid-reference";

		public static string InvalidSchema => "invalid-schema";

		public static string InvalidConfiguration => "invalid-configuration";

		public static string InvalidContentType => "invalid-content-type";
		
		public static string InvalidZipSize => "invalid-archive-size";
	}
}