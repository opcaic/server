namespace OPCAIC.ApiService.ModelValidationHandling
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

		public static string MinValueError => "field-min-value";

		public static string RequiredError => "field-required";

		public static string InvalidEmailError => "field-invalid-email";

		public static string RangeError => "field-range-error";

		#endregion

		#region Conflicts

		public static string GameNameConflict => "game-name-conflict";

		public static string UserEmailConflict => "user-email-conflict";

		public static string UserUsernameConflict => "user-username-conflict";

		public static string OldPasswordConflict => "old-password-conflict";

		public static string PasswordKeyConflict => "password-key-conflict";

		public static string UserWithEmailNotFound => "user-with-email-not-found";

		#endregion
	}
}