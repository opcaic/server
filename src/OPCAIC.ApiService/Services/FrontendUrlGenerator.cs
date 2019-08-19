using Microsoft.Extensions.Options;
using OPCAIC.ApiService.Configs;

namespace OPCAIC.ApiService.Services
{
	internal class FrontendUrlGenerator : IFrontendUrlGenerator
	{
		private readonly UrlGeneratorConfiguration config;

		public FrontendUrlGenerator(IOptions<UrlGeneratorConfiguration> config)
		{
			this.config = config.Value;
		}

		/// <inheritdoc />
		public string PasswordResetLink(long userId, string token)
		{
			return $"{config.FrontendUrl}/passwordReset?userId={userId}&token={token}";
		}

		/// <inheritdoc />
		public string EmailConfirmLink(long userId, string token)
		{
			return $"{config.FrontendUrl}/confirmEmail?userId={userId}&token={token}";
		}

		/// <inheritdoc />
		public string TournamentInviteUrl(long tournamentId)
		{
			return $"{config.FrontendUrl}/tournament/{tournamentId}";
		}
	}
}