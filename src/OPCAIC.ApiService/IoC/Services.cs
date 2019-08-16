﻿using Microsoft.Extensions.DependencyInjection;
using OPCAIC.ApiService.Services;
using OPCAIC.Infrastructure.Emails;
using OPCAIC.Services;

namespace OPCAIC.ApiService.IoC
{
	public static class Services
	{
		public static void AddServices(this IServiceCollection services) => services
			.AddTransient<IUserService, UserService>()
			.AddTransient<IStorageService, StorageService>()
			.AddTransient<IGamesService, GamesService>()
			.AddTransient<IEmailService, EmailService>()
			.AddTransient<ITokenService, TokenService>()
			.AddTransient<ITournamentsService, TournamentsService>()
			.AddTransient<IDocumentService, DocumentService>()
			.AddTransient<IMatchService, MatchService>();
	}
}