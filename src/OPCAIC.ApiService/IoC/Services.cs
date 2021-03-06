﻿using Microsoft.Extensions.DependencyInjection;
using OPCAIC.ApiService.Interfaces;
using OPCAIC.ApiService.ModelValidationHandling;
using OPCAIC.ApiService.Services;
using OPCAIC.ApiService.Utils;
using OPCAIC.Application.Emails;
using OPCAIC.Application.Extensions;
using OPCAIC.Application.Interfaces;
using OPCAIC.Application.Services;
using OPCAIC.Common;
using OPCAIC.Infrastructure;
using OPCAIC.Infrastructure.Emails;

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
				.AddTransient<TournamentProcessor.Job>()
				.AddHostedService<EmailCronService>()
				.AddTransient<IEmailSender, EmailSender>()
				.AddSingleton<ILogStorageService, LogStorageService>()
				.AddSingleton<IStorageService, StorageService>()
				.AddSingleton<ITimeService, MachineTimeService>()
				.AddScoped<IEmailService, EmailService>()
				.AddScoped<IUserManager, UserManager>()
				.AddScoped<ISubmissionService, SubmissionService>()
				.AddScoped<IFrontendUrlGenerator, UrlGenerator>()
				.AddScoped<IWorkerUrlGenerator, UrlGenerator>()
				.AddScoped<IModelValidationService, ModelValidationService>()
				.AddScoped<IWorkerService, WorkerService>()
				.AddScoped<IBrokerService, BrokerService>()
				.AddScoped<ISubmissionScoreService, SubmissionScoreService>()
				.AddTransient<IDatabaseSeed, DatabaseSeed>();
		}
	}
}