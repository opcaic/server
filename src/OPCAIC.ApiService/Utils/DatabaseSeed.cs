using System;
using System.Linq;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OPCAIC.ApiService.Configs;
using OPCAIC.ApiService.Exceptions;
using OPCAIC.ApiService.Services;
using OPCAIC.ApiService.Users.Commands;
using OPCAIC.Application.Emails.Templates;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enumerations;
using OPCAIC.Domain.Enums;
using OPCAIC.Persistence;

namespace OPCAIC.ApiService.Utils
{
	public interface IDatabaseSeed
	{
		/// <summary>
		///     Seeds the database with initial values.
		/// </summary>
		/// <returns></returns>
		bool DoSeed();
	}

	public class DatabaseSeed
		: IDatabaseSeed
	{
		private readonly DataContext context;
		private readonly IWebHostEnvironment environment;
		private readonly ILogger<DatabaseSeed> logger;
		private readonly SeedConfig seedConfig;
		private readonly IServiceProvider services;
		private readonly UserManager userManager;

		/// <inheritdoc />
		public DatabaseSeed(ILogger<DatabaseSeed> logger, DataContext context,
			IWebHostEnvironment environment, UserManager userManager, IServiceProvider services,
			IOptions<SeedConfig> seedConfig)
		{
			this.logger = logger;
			this.context = context;
			this.environment = environment;
			this.userManager = userManager;
			this.services = services;
			this.seedConfig = seedConfig.Value;
		}

		public bool DoSeed()
		{
			// apply pending migrations

			if (context.Database.IsSqlite())
			{
				// Hack for functional tests: SQLite does not support some migrations generated for postgres
				context.Database.EnsureCreated();
			}
			else
			{
				logger.LogInformation("Checking for database migrations");
				var migrations = context.Database.GetPendingMigrations().ToArray();
				if (migrations.Length > 0)
				{
					logger.LogInformation("Applying migrations: " + string.Join(", ", migrations));
					context.Database.Migrate();
					logger.LogInformation("Migration successful");
				}
			}

			if (userManager.Users.Any())
			{
				// database already initialized
				return true;
			}

			logger.LogInformation("Preparing database before first usage.");

			if (seedConfig.AdminEmail == null ||
				seedConfig.AdminPassword == null ||
				seedConfig.AdminUsername == null)
			{
				logger.LogCritical(
					"You must provide --Seed.AdminUsername, --Seed:AdminEmail and --Seed:AdminPassword parameters on first startup.");
				return false;
			}

			logger.LogInformation("Seeding email templates.");
			AddEmailTemplates(context);

			logger.LogInformation(
				$"Seeding database with user '{seedConfig.AdminUsername}' and email '{seedConfig.AdminEmail}'");

			var command = new CreateUserCommand
			{
				LocalizationLanguage = LocalizationLanguage.EN,
				Email = seedConfig.AdminEmail,
				Password = seedConfig.AdminPassword,
				Username = seedConfig.AdminUsername
			};

			try
			{
				services.GetRequiredService<IMediator>().Send(command).GetAwaiter().GetResult();
			}
			catch (ModelValidationException e)
			{
				logger.LogCritical("Failed to create admin account:\n" +
					string.Join("\n", e.ValidationErrors.Select(e => e.Message)));
				return false;
			}

			logger.LogInformation("Setting admin privileges");
			context.Users.Single().Role = UserRole.Admin;
			context.SaveChanges();

			logger.LogInformation("Successfully seeded the database.");
			return true;
		}

		public static void AddEmailTemplates(DataContext context)
		{
			context.EmailTemplates.AddRange(
				new EmailTemplate
				{
					LanguageCode = LocalizationLanguage.CZ,
					Name = EmailType.UserVerification,
					SubjectTemplate = "Ověření emailu",
					BodyTemplate =
						"<html><body>Ověřte svůj email na této adrese: {{VerificationUrl}}.</body></html>"
				},
				new EmailTemplate
				{
					LanguageCode = LocalizationLanguage.EN,
					Name = EmailType.UserVerification,
					SubjectTemplate = "Email verification",
					BodyTemplate =
						"<html><body>Verify your email by clicking on address: {{VerificationUrl}}.</body></html>"
				},
				new EmailTemplate
				{
					LanguageCode = LocalizationLanguage.CZ,
					Name = EmailType.PasswordReset,
					SubjectTemplate = "Zapomenuté heslo",
					BodyTemplate =
						"<html><body>Přesuňte se na stránku změny hesla kliknutím na odkaz: {{ResetUrl}}.</body></html>"
				},
				new EmailTemplate
				{
					LanguageCode = LocalizationLanguage.EN,
					Name = EmailType.PasswordReset,
					SubjectTemplate = "Password reset",
					BodyTemplate =
						"<html><body>Move to page, where you can change your password by clicking on: {{ResetUrl}}.</body></html>"
				},
				new EmailTemplate
				{
					LanguageCode = LocalizationLanguage.CZ,
					Name = EmailType.TournamentInvitation,
					SubjectTemplate = "Přídání do turnaje",
					BodyTemplate =
						"<html><body>Byli ste pozváni do následujícího turnaje: {{TournamentUrl}}.</body></html>"
				},
				new EmailTemplate
				{
					LanguageCode = LocalizationLanguage.EN,
					Name = EmailType.TournamentInvitation,
					SubjectTemplate = "Tournament invitation",
					BodyTemplate =
						"<html><body>You were invited to following tournament: {{TournamentUrl}}.</body></html>"
				},
				new EmailTemplate
				{
					LanguageCode = LocalizationLanguage.CZ,
					Name = EmailType.SubmissionValidationFailed,
					SubjectTemplate = "Validace vaší submission selhala",
					BodyTemplate =
						"<html><body>Vaše submission není validní a nemůže se zúčastnit turnaje: {{SubmissionUrl}}.</body></html>"
				},
				new EmailTemplate
				{
					LanguageCode = LocalizationLanguage.EN,
					Name = EmailType.SubmissionValidationFailed,
					SubjectTemplate = "Submission validation failed",
					BodyTemplate =
						"<html><body>Your submission is not valid for the tournament: {{SubmissionUrl}}.</body></html>"
				},
				new EmailTemplate
				{
					LanguageCode = LocalizationLanguage.CZ,
					Name = EmailType.TournamentFinished,
					SubjectTemplate = "Vyhodnocení turnaje '{{TournamentName}}' skončilo",
					BodyTemplate =
						"<html><body>Turnaj {{TournamentName}} byl ukončen. Výsledky jsou k dispozici na odkazu {{TournamentUrl}}.</body></html>"
				},
				new EmailTemplate
				{
					LanguageCode = LocalizationLanguage.EN,
					Name = EmailType.TournamentFinished,
					SubjectTemplate = "Tournament '{{TournamentName}}' has finished",
					BodyTemplate =
						"<html><body>Tournament {{TournamentName}} has finished. Results are available at {{TournamentUrl}}.</body></html>"
				});

			context.SaveChanges();
		}
	}
}