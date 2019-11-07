using System;
using Microsoft.Extensions.Options;
using OPCAIC.ApiService.Configs;
using OPCAIC.Application.Interfaces;

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
		public string TournamentPageLink(long tournamentId)
		{
			return $"{config.FrontendUrl}/tournament/{tournamentId}";
		}

		/// <inheritdoc />
		public string SubmissionPageLink(long tournamentId, long submissionId)
		{
			return $"{config.FrontendUrl}/tournament/{tournamentId}/submission/{submissionId}";
		}

		/// <inheritdoc />
		public string GenerateAdditionalFilesUrl(long tournamentId)
		{
			return $"tournaments/{tournamentId}/files";
		}
	}
}