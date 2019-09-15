namespace OPCAIC.Application.Interfaces
{
	public interface IFrontendUrlGenerator
	{
		/// <summary>
		///     Generates url for resetting user's password.
		/// </summary>
		/// <param name="email"></param>
		/// <param name="token">Password reset token issued by the application.</param>
		/// <returns></returns>
		string PasswordResetLink(string email, string token);

		/// <summary>
		///     Generates url for confirming user's email address.
		/// </summary>
		/// <param name="email"></param>
		/// <param name="token">Password reset token issued by the application.</param>
		/// <returns></returns>
		string EmailConfirmLink(string email, string token);

		/// <summary>
		///     Generates url for the tournament main page.
		/// </summary>
		/// <param name="tournamentId">Id of the tournament.</param>
		/// <returns></returns>
		string TournamentPageLink(long tournamentId);
	}
}