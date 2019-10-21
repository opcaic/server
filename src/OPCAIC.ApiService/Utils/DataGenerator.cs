using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using OPCAIC.ApiService.Services;
using OPCAIC.Application.Emails.Templates;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enumerations;
using OPCAIC.Domain.Enums;
using OPCAIC.Persistence;

namespace OPCAIC.ApiService.Utils
{
	public class DataGenerator
	{
		public static void Initialize(IServiceProvider serviceProvider)
		{
			var userManager = serviceProvider.GetRequiredService<UserManager>();
			if (userManager.Users.Any())
			{
				// already initialized
				return;
			}

			var userAdmin = new User
			{
				FirstName = "Admin",
				LastName = "Opcaic",
				UserName = "admin",
				Role = UserRole.Admin,
				Email = "admin@opcaic.com",
				EmailConfirmed = true,
				LocalizationLanguage = LocalizationLanguage.EN
			};

			userManager.CreateAsync(userAdmin, "Password").GetAwaiter().GetResult();

			var context = serviceProvider.GetRequiredService<DataContext>();
			AddEmailTemplates(context);
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