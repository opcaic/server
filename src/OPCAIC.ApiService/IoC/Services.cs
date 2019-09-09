﻿using Microsoft.Extensions.DependencyInjection;
using OPCAIC.ApiService.ModelValidationHandling;
using OPCAIC.ApiService.Services;
using OPCAIC.Infrastructure.Emails;
using OPCAIC.Services;
using OPCAIC.Services.Extensions;

namespace OPCAIC.ApiService.IoC
{
	public static class Services
	{
		public static void AddServices(this IServiceCollection services)
		{
			services
				.AddMatchGenerators()
				.AddHostedService<BrokerReactor>()
				.AddHostedService<TournamentProcessor>()
				.AddHostedService<EmailCronService>()
				.AddTransient<EmailSender>()
				.AddSingleton<ILogStorageService, LogStorageService>()
				.AddSingleton<IStorageService, StorageService>()
				.AddScoped<IEmailService, EmailService>()
				.AddScoped<IUserManager, UserManager>()
				.AddScoped<IGamesService, GamesService>()
				.AddScoped<ITournamentsService, TournamentsService>()
				.AddScoped<ITournamentParticipantsService, TournamentParticipantsService>()
				.AddScoped<IDocumentService, DocumentService>()
				.AddScoped<ISubmissionService, SubmissionService>()
				.AddScoped<IFrontendUrlGenerator, UrlGenerator>()
				.AddScoped<IWorkerUrlGenerator, UrlGenerator>()
				.AddSingleton<IModelValidationService, ModelValidationService>()
				.AddScoped<IMatchService, MatchService>()
				.AddScoped<ISubmissionValidationService, SubmissionValidationService>()
				.AddScoped<IMatchExecutionService, MatchExecutionService>()
				.AddScoped<IWorkerService, WorkerService>()
				.AddScoped<IBrokerService, BrokerService>()
				.AddScoped<ILeaderboardService, LeaderboardService>();
		}
	}
}