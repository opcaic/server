using System;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using OPCAIC.ApiService.Configs;
using OPCAIC.ApiService.Interfaces;
using OPCAIC.Application.Interfaces;

namespace OPCAIC.ApiService.Services
{
	public class WorkerService : IWorkerService
	{
		private readonly IStorageService storage;
		private readonly IJwtTokenService tokenService;
		private readonly IWorkerUrlGenerator urlGenerator;
		private readonly TimeSpan tokenLifetime;

		public WorkerService(IStorageService storage, IJwtTokenService tokenService, IOptions<SecurityConfiguration> conf, IWorkerUrlGenerator urlGenerator)
		{
			this.storage = storage;
			this.tokenService = tokenService;
			this.urlGenerator = urlGenerator;
			tokenLifetime = TimeSpan.FromMinutes(conf.Value.WorkerTokenExpirationMinutes);
		}

		public string GetAdditionalFilesUrl(long tournamentId)
		{
			var stream = storage.ReadTournamentAdditionalFiles(tournamentId);
			if (stream == null)
				return null;

			stream.Dispose();
			return urlGenerator.GenerateAdditionalFilesUrl(tournamentId);
		}

		/// <inheritdoc />
		public string GenerateWorkerToken(ClaimsIdentity identity)
		{
			return tokenService.CreateToken(tokenLifetime, identity);
		}
	}
}