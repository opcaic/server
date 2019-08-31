using Microsoft.Extensions.Options;
using OPCAIC.ApiService.Configs;

namespace OPCAIC.ApiService.Services
{
	internal class UrlGenerator : IFrontendUrlGenerator, IWorkerUrlGenerator
	{
		private readonly UrlGeneratorConfiguration config;

		public UrlGenerator(IOptions<UrlGeneratorConfiguration> config)
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

		/// <inheritdoc />
		public string GenerateAdditionalFilesUrl(long tournamentId)
		{
			return $"api/tournaments/{tournamentId}/files";
		}
	}
}