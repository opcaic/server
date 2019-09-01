using System;
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
		public string PasswordResetLink(string email, string token)
		{
			return $"{config.FrontendUrl}/reset-password?email={email}&token={Uri.EscapeDataString(token)}";
		}

		/// <inheritdoc />
		public string EmailConfirmLink(string email, string token)
		{
			return $"{config.FrontendUrl}/confirm-email?email={email}&token={Uri.EscapeDataString(token)}";
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