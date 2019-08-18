namespace OPCAIC.ApiService.Services
{
	public interface IFrontendUrlGenerator
	{
		/// <summary>
		///     Generates url for resetting user's password.
		/// </summary>
		/// <param name="userId">Id of the user.</param>
		/// <param name="token">Password reset token issued by the application.</param>
		/// <returns></returns>
		string PasswordResetLink(long userId, string token);

		/// <summary>
		///     Generates url for confirming user's email address.
		/// </summary>
		/// <param name="userId">Id of the user.</param>
		/// <param name="token">Password reset token issued by the application.</param>
		/// <returns></returns>
		string EmailConfirmLink(long userId, string token);
	}
}