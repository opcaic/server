using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OPCAIC.Infrastructure.DbContexts;
using OPCAIC.Infrastructure.Entities;
using OPCAIC.Infrastructure.Enums;

namespace OPCAIC.ApiService.Utils
{
	public class DataGenerator
	{
		public static void Initialize(IServiceProvider serviceProvider)
		{
			using (var context = new DataContext(
				serviceProvider.GetRequiredService<DbContextOptions<DataContext>>()))
			{
				// Look for any board games.
				if (context.Set<Tournament>().Any())
				{
					return; // Data was already seeded
				}

				context.Set<Game>().AddRange(
					new Game {Name = "Chess", Created = DateTime.Now},
					new Game {Name = "2048", Created = DateTime.Now},
					new Game {Name = "Dota", Created = DateTime.Now},
					new Game {Name = "Tic-Tao-Toe", Created = DateTime.Now}
				);

				context.SaveChanges();

				context.Set<Tournament>().AddRange(
					new Tournament
					{
						Name = "Chess ELO tournament",
						Game = context.Set<Game>().Single(x => x.Name == "Chess"),
						GameId = context.Set<Game>().Single(x => x.Name == "Chess").Id,
						Created = DateTime.Now,
						Format = TournamentFormat.Elo,
						RankingStrategy = TournamentRankingStrategy.Maximum,
						Scope = TournamentScope.Ongoing
					},
					new Tournament
					{
						Name = "2048 single player",
						Game = context.Set<Game>().Single(x => x.Name == "2048"),
						GameId = context.Set<Game>().Single(x => x.Name == "2048").Id,
						Created = DateTime.Now,
						Format = TournamentFormat.SinglePlayer,
						RankingStrategy = TournamentRankingStrategy.Maximum,
						Scope = TournamentScope.Ongoing
					},
					new Tournament
					{
						Name = "Summer Dota single elimination",
						Game = context.Set<Game>().Single(x => x.Name == "Dota"),
						GameId = context.Set<Game>().Single(x => x.Name == "Dota").Id,
						Created = DateTime.Now,
						Format = TournamentFormat.SingleElimination,
						RankingStrategy = TournamentRankingStrategy.Maximum,
						Scope = TournamentScope.Deadline
					},
					new Tournament
					{
						Name = "Ongoing Dota ELO",
						Game = context.Set<Game>().Single(x => x.Name == "Dota"),
						GameId = context.Set<Game>().Single(x => x.Name == "Dota").Id,
						Created = DateTime.Now,
						Format = TournamentFormat.Elo,
						RankingStrategy = TournamentRankingStrategy.Maximum,
						Scope = TournamentScope.Ongoing
					}
				);

				context.Set<User>().AddRange(
					new User
					{
						FirstName = "Admin",
						LastName = "Opcaic",
						Created = DateTime.Now,
						Username = "admin",
						RoleId = (long)UserRole.Admin,
						PasswordHash = "3CFfbIw0//kGGeW5x26Bu/3FA6IqKAogIbf1fL/bLsg=",
						Email = "admin@opcaic.com",
						EmailVerified = true,
						LocalizationLanguage = "cs"
					},
					new User
					{
						FirstName = "Organizer",
						LastName = "Opcaic",
						Created = DateTime.Now,
						Username = "organizer",
						RoleId = (long)UserRole.Organizer,
						PasswordHash = "3CFfbIw0//kGGeW5x26Bu/3FA6IqKAogIbf1fL/bLsg=",
						Email = "organizer@opcaic.com",
						EmailVerified = false,
						LocalizationLanguage = "en"
					},
					new User
					{
						FirstName = "User",
						LastName = "Opcaic",
						Created = DateTime.Now,
						Username = "user",
						RoleId = (long)UserRole.User,
						PasswordHash = "3CFfbIw0//kGGeW5x26Bu/3FA6IqKAogIbf1fL/bLsg=",
						Email = "user@opcaic.com",
						EmailVerified = true,
						LocalizationLanguage = "cs"
					});

				AddEmailTemplates(context);

				context.SaveChanges();

				context.Set<Document>().AddRange(
					new Document
					{
						Name = "2048 short description",
						Tournament =
							context.Set<Tournament>()
								.Single(x => x.Name == "2048 single player"),
						TournamentId =
							context.Set<Tournament>()
								.Single(x => x.Name == "2048 single player").Id,
						Content =
							"2048 is a really _easy_ and _fun_ game. The only rule is that you can merge **two blocks with same number** to create a block with **twice as big number**. The more blocks you merge the blocks, the better!"
					},
					new Document
					{
						Name = "ELO short description",
						Tournament =
							context.Set<Tournament>()
								.Single(x => x.Name == "Chess ELO tournament"),
						TournamentId =
							context.Set<Tournament>()
								.Single(x => x.Name == "Chess ELO tournament").Id,
						Content =
							"Elo is a statistical method of ranking players' abilities. In that system, every player is given a number of **Elo points** representing his skill, and after each match, points of _both_ participating players are updated according to the _expectability_ of the match outcome."
					});
				context.SaveChanges();

				context.Set<Document>().AddRange(
					new Document
					{
						Name = "2048 short description",
						Tournament =
							context.Set<Tournament>()
								.Single(x => x.Name == "2048 single player"),
						TournamentId =
							context.Set<Tournament>()
								.Single(x => x.Name == "2048 single player").Id,
						Content =
							"2048 is a really _easy_ and _fun_ game. The only rule is that you can merge **two blocks with same number** to create a block with **twice as big number**. The more blocks you merge the blocks, the better!"
					},
					new Document
					{
						Name = "ELO short description",
						Tournament =
							context.Set<Tournament>()
								.Single(x => x.Name == "Chess ELO tournament"),
						TournamentId =
							context.Set<Tournament>()
								.Single(x => x.Name == "Chess ELO tournament").Id,
						Content =
							"Elo is a statistical method of ranking players' abilities. In that system, every player is given a number of **Elo points** representing his skill, and after each match, points of _both_ participating players are updated according to the _expectability_ of the match outcome."
					});
				context.SaveChanges();

				var users = new List<User>
				{
					context.Set<User>().Single(x => x.Username == "admin"),
					context.Set<User>().Single(x => x.Username == "organizer")
				};
				context.Set<Match>().AddRange(
					new Match
					{
						TournamentId = context.Set<Tournament>()
							.Single(x => x.Name == "Chess ELO tournament").Id,
						Tournament = context.Set<Tournament>()
							.Single(x => x.Name == "Chess ELO tournament"),
						Index = 1,
						Participators = new List<UserParticipation>
						{
							new UserParticipation
							{
								User = context.Set<User>()
									.Single(x => x.Username == "admin"),
								UserId = context.Set<User>()
									.Single(x => x.Username == "admin").Id
							},
							new UserParticipation
							{
								User = context.Set<User>()
									.Single(x => x.Username == "organizer"),
								UserId = context.Set<User>()
									.Single(x => x.Username == "organizer").Id
							}
						},
						Results = new List<SubmissionMatchResult>
						{
							new SubmissionMatchResult
							{
								Score = 1.0,
								AdditionalDataJson =
									"{message = \"Admin won\"}"
							}
						}
					},
					new Match
					{
						TournamentId = context.Set<Tournament>()
							.Single(x => x.Name == "Chess ELO tournament").Id,
						Tournament = context.Set<Tournament>()
							.Single(x => x.Name == "Chess ELO tournament"),
						Index = 2,
						Participators = new List<UserParticipation>
						{
							new UserParticipation
							{
								User = context.Set<User>()
									.Single(x => x.Username == "admin"),
								UserId = context.Set<User>()
									.Single(x => x.Username == "admin").Id
							},
							new UserParticipation
							{
								User = context.Set<User>()
									.Single(x => x.Username == "organizer"),
								UserId = context.Set<User>()
									.Single(x => x.Username == "organizer").Id
							}
						},
						Results = new List<SubmissionMatchResult>
						{
							new SubmissionMatchResult
							{
								Score = -1.0,
								AdditionalDataJson =
									"{message = \"Organizer won\"}"
							}
						}
					});
				context.SaveChanges();
			}
		}

		private static void AddEmailTemplates(DataContext context)
		{
			context.EmailTemplates.AddRange(
				new EmailTemplate
				{
					LanguageCode = "cs",
					Name = "userVerificationEmail",
					SubjectTemplate = "Ověření emailu",
					BodyTemplate =
						"<html><body>Ověřte svůj email na této adrese: {{VerificationUrl}}.</body></html>"
				},
				new EmailTemplate
				{
					LanguageCode = "en",
					Name = "userVerificationEmail",
					SubjectTemplate = "Email verification",
					BodyTemplate =
						"<html><body>Verify your email by clicking on address: {{VerificationUrl}}.</body></html>"
				},
				new EmailTemplate
				{
					LanguageCode = "cs",
					Name = "passwordResetEmail",
					SubjectTemplate = "Zapomenuté heslo",
					BodyTemplate =
						"<html><body>Přesuňte se na stránku změny hesla kliknutím na odkaz: {{ResetUrl}}.</body></html>"
				},
				new EmailTemplate
				{
					LanguageCode = "en",
					Name = "passwordResetEmail",
					SubjectTemplate = "Password reset",
					BodyTemplate =
						"<html><body>Move to page, where you can change your password by clicking on: {{ResetUrl}}.</body></html>"
				});
		}
	}
}