using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OPCAIC.ApiService.Services;
using OPCAIC.Infrastructure.DbContexts;
using OPCAIC.Infrastructure.Dtos.MatchExecutions;
using OPCAIC.Infrastructure.Dtos.Submissions;
using OPCAIC.Infrastructure.Dtos.SubmissionValidations;
using OPCAIC.Infrastructure.Entities;
using OPCAIC.Infrastructure.Enums;
using OPCAIC.Services;

namespace OPCAIC.ApiService.Utils
{
	public class DataGenerator
	{
		private static Mapper mapper = new Mapper(MapperConfigurationFactory.Create());

		public static void WriteRandomZipArchive(Stream archive)
		{
			using (var archive1 = new ZipArchive(archive, ZipArchiveMode.Create, true))
			using (var writer = new StreamWriter(archive1.CreateEntry("a.txt").Open()))
			{
				writer.WriteLine("Random content");
			}
		}

		public static void EnsureSubmissionArchiveExists(IStorageService storage, Submission sub)
		{
			var storageDto = mapper.Map<SubmissionStorageDto>(sub);

			var archive = storage.ReadSubmissionArchive(storageDto);
			if (archive != null)
			{
				archive.Dispose();
				return; // already exists
			}

			// write something so that we have at least some file
			using (archive = storage.WriteSubmissionArchive(storageDto))
			{
				WriteRandomZipArchive(archive);
			}
		}

		public static void EnsureSubmissionValidationResultExists(IStorageService storage,
			SubmissionValidation sub)
		{
			var storageDto = mapper.Map<SubmissionValidationStorageDto>(sub);

			var archive = storage.ReadSubmissionValidationResultArchive(storageDto);
			if (archive != null)
			{
				archive.Dispose();
				return; // already exists
			}

			// write something so that we have at least some file
			using (archive = storage.WriteSubmissionValidationResultArchive(storageDto))
			{
				WriteRandomZipArchive(archive);
			}
		}

		public static void EnsureMatchExecutionResultExists(IStorageService storage,
			MatchExecution execution)
		{
			var storageDto = mapper.Map<MatchExecutionStorageDto>(execution);

			var archive = storage.ReadMatchResultArchive(storageDto);
			if (archive != null)
			{
				archive.Dispose();
				return; // already exists
			}

			// write something so that we have at least some file
			using (archive = storage.WriteMatchResultArchive(storageDto))
			{
				WriteRandomZipArchive(archive);
			}
		}

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

				// delete all existing files to prevent inconsistent state
				var conf = serviceProvider.GetRequiredService<IOptions<StorageConfiguration>>().Value;
				if (Directory.Exists(conf.Directory))
					Directory.Delete(conf.Directory, true);

				// recreates directory structure
				var storage = serviceProvider.GetRequiredService<IStorageService>();


				var gameChess = new Game
				{
					Name = "Chess",
					Key = "chess",
					Created = DateTime.Now,
					ImageUrl = "https://images.chesscomfiles.com/uploads/v1/article/17623.87bb05cd.668x375o.47d81802f1eb@2x.jpeg",
					DefaultTournamentImageOverlay = 0.7f,
					DefaultTournamentImage = "https://images.chesscomfiles.com/uploads/v1/article/17623.87bb05cd.668x375o.47d81802f1eb@2x.jpeg",
					DefaultTournamentThemeColor = "#491e01",
					ConfigurationSchema = "{}",
				};
				var game2048 = new Game
				{
					Name = "2048",
					Key = "2048",
					Created = DateTime.Now,
					ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/6/64/2048_Screenshot.png",
					DefaultTournamentImageOverlay = 0.7f,
					DefaultTournamentImage = "https://upload.wikimedia.org/wikipedia/commons/6/64/2048_Screenshot.png",
					DefaultTournamentThemeColor = "#f67c5f",
					ConfigurationSchema = "{}",
				};
				var gameDota = new Game
				{
					Name = "Dota 2",
					Key = "dota2",
					Created = DateTime.Now,
					ImageUrl = "https://wallpapercave.com/wp/V8Ee1Bm.jpg",
					DefaultTournamentImageOverlay = 0.2f,
					DefaultTournamentImage = "https://wallpapercave.com/wp/V8Ee1Bm.jpg",
					DefaultTournamentThemeColor = "#2d0f0a",
					ConfigurationSchema = "{}",
				};
				var gameMario = new Game
				{
					Name = "Super Mario Bros.",
					Key = "mario",
					Created = DateTime.Now,
					ImageUrl = "https://cdn02.nintendo-europe.com/media/images/10_share_images/games_15/nintendo_ds_22/SI_NDS_NewSuperMarioBrosDS_image1600w.jpg",
					DefaultTournamentImageOverlay = 0.6f,
					DefaultTournamentImage = "https://cdn02.nintendo-europe.com/media/images/10_share_images/games_15/nintendo_ds_22/SI_NDS_NewSuperMarioBrosDS_image1600w.jpg",
					DefaultTournamentThemeColor = "#db0522",
					ConfigurationSchema = "{}",
				};

				context.Set<Game>().AddRange(gameChess, game2048, gameDota, gameMario);
				context.SaveChanges();

				var tournamentChessElo = new Tournament
				{
					Name = "Chess ELO tournament",
					Game = gameChess,
					Created = DateTime.Now,
					Format = TournamentFormat.Elo,
					RankingStrategy = TournamentRankingStrategy.Maximum,
					Scope = TournamentScope.Ongoing,
					Availability = TournamentAvailability.Private,
					State = TournamentState.Published,
					Configuration = "{}",
				};
				var tournament2048 = new Tournament
				{
					Name = "2048 single player",
					Game = game2048,
					Created = DateTime.Now,
					Format = TournamentFormat.SinglePlayer,
					RankingStrategy = TournamentRankingStrategy.Maximum,
					Scope = TournamentScope.Ongoing,
					Availability = TournamentAvailability.Private,
					State = TournamentState.Published,
					Configuration = "{}",
				};
				var tournamentDotaSe = new Tournament
				{
					Name = "Summer Dota single elimination",
					Game = gameDota,
					Created = DateTime.Now,
					Format = TournamentFormat.SingleElimination,
					RankingStrategy = TournamentRankingStrategy.Maximum,
					Scope = TournamentScope.Deadline,
					Deadline = DateTime.Now.AddDays(6),
					Availability = TournamentAvailability.Public,
					State = TournamentState.Published,
					Configuration = "{}",
				};
				var tournamentDotaElo = new Tournament
				{
					Name = "Ongoing Dota ELO",
					Game = gameDota,
					Created = DateTime.Now,
					Format = TournamentFormat.Elo,
					RankingStrategy = TournamentRankingStrategy.Maximum,
					Scope = TournamentScope.Ongoing,
					Availability = TournamentAvailability.Public,
					State = TournamentState.Published,
					Configuration = "{}",
				};
				var tournamentMario = new Tournament
				{
					Name = "Super Mario Bros.",
					Game = gameMario,
					Created = DateTime.Now,
					Format = TournamentFormat.SinglePlayer,
					RankingStrategy = TournamentRankingStrategy.Maximum,
					Scope = TournamentScope.Deadline,
					Deadline = DateTime.Now.AddDays(17),
					Availability = TournamentAvailability.Public,
					State = TournamentState.Published,
					Configuration = "{}",
				};

				context.Set<Tournament>().AddRange(
					tournamentChessElo,
					tournament2048,
					tournamentDotaSe,
					tournamentDotaElo,
					tournamentMario
				);

				var mgr = serviceProvider.GetRequiredService<UserManager>();
				var userAdmin = new User
				{
					FirstName = "Admin",
					LastName = "Opcaic",
					Created = DateTime.Now,
					UserName = "admin",
					RoleId = (long)UserRole.Admin,
					Email = "admin@opcaic.com",
					EmailConfirmed = true,
					LocalizationLanguage = "cs"
				};

				var userOrganizer = new User
				{
					FirstName = "Organizer",
					LastName = "Opcaic",
					Created = DateTime.Now,
					UserName = "organizer",
					RoleId = (long)UserRole.Organizer,
					Email = "organizer@opcaic.com",
					EmailConfirmed = true,
					LocalizationLanguage = "en"
				};

				var user = new User
				{
					FirstName = "User",
					LastName = "Opcaic",
					Created = DateTime.Now,
					UserName = "user",
					RoleId = (long)UserRole.User,
					Email = "user@opcaic.com",
					EmailConfirmed = true,
					LocalizationLanguage = "cs"
				};

				mgr.CreateAsync(userAdmin, "Password").Wait();
				userAdmin = context.Users.Find(userAdmin.Id);
				mgr.CreateAsync(userOrganizer, "Password").Wait();
				userOrganizer = context.Users.Find(userOrganizer.Id);
				mgr.CreateAsync(user, "Password").Wait();
				user = context.Users.Find(user.Id);

				AddEmailTemplates(context);


				context.Set<Document>().AddRange(
					new Document
					{
						Name = "2048 short description",
						Tournament = tournament2048,
						Content =
							"2048 is a really _easy_ and _fun_ game. The only rule is that you can merge **two blocks with same number** to create a block with **twice as big number**. The more blocks you merge the blocks, the better!"
					},
					new Document
					{
						Name = "ELO short description",
						Tournament = tournamentChessElo,
						Content =
							"2048 is a really _easy_ and _fun_ game. The only rule is that you can merge **two blocks with same number** to create a block with **twice as big number**. The more blocks you merge the blocks, the better!"
					},
					new Document
					{
						Name = "ELO short description",
						Tournament = tournamentChessElo,
						Content =
							"Elo is a statistical method of ranking players' abilities. In that system, every player is given a number of **Elo points** representing his skill, and after each match, points of _both_ participating players are updated according to the _expectability_ of the match outcome."
					});


				var submissionChessAdmin = CreateSubmission(userAdmin, tournamentChessElo);
				var submissionChessOrganizer = CreateSubmission(userOrganizer, tournamentChessElo);
				context.Submissions.AddRange(submissionChessAdmin, submissionChessOrganizer);

				context.SaveChanges();

				var matchChessAdminOrganizer = CreateMatch(tournamentChessElo, 1,
					submissionChessAdmin, submissionChessOrganizer);

				AddExecution(matchChessAdminOrganizer);
				context.Set<Match>().AddRange(matchChessAdminOrganizer);
				context.SaveChanges();

				// add necessary files
				foreach (var submission in context.Submissions)
				{
					EnsureSubmissionArchiveExists(storage, submission);
				}

				foreach (var validation in context.SubmissionValidations)
				{
					EnsureSubmissionValidationResultExists(storage, validation);
				}

				foreach (var execution in context.MatchExecutions)
				{
					EnsureMatchExecutionResultExists(storage, execution);
				}
			}
		}

		private static Match CreateMatch(Tournament tournament, int index, params Submission[] participants)
		{
			return new Match
			{
				Tournament = tournament,
				Index = index,
				Participations =
					participants.Select(s => new SubmissionParticipation {Submission = s})
						.ToList(),
				Executions = new List<MatchExecution>() // executions to be added separately.
			};
		}

		private static MatchExecution AddExecution(Match match)
		{
			int i = 0;
			var matchExecution = new MatchExecution()
			{
				AdditionalData = "{ 'time': 10000 }",
				BotResults = match.Participations.Select(s => new SubmissionMatchResult
				{
					Submission = s.Submission,
					CompilerResult = EntryPointResult.Success,
					Score = i++,
					AdditionalData = "{ 'moves': 10 }"
				}).ToList()
			};
			match.Executions.Add(matchExecution);
			return matchExecution;
		}

		private static Submission CreateSubmission(User author, Tournament tournament)
		{
			return new Submission
			{
				Author = author,
				Tournament = tournament,
				IsActive = true,
				Validations = new List<SubmissionValidation>
					{
						new SubmissionValidation
						{
							CheckerResult = EntryPointResult.Success,
							CompilerResult = EntryPointResult.Success,
							ValidatorResult = EntryPointResult.Success,
							State = WorkerJobState.Finished,
							Executed = DateTime.Now
						}
					}
			};
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
				},
				new EmailTemplate
				{
					LanguageCode = "cs",
					Name = "tournamentInvitationEmail",
					SubjectTemplate = "Přídání do turnaje",
					BodyTemplate =
						"<html><body>Byli ste pozváni do následujícího turnaje: {{TournamentUrl}}.</body></html>"
				},
				new EmailTemplate
				{
					LanguageCode = "en",
					Name = "tournamentInvitationEmail",
					SubjectTemplate = "Tournament invitation",
					BodyTemplate =
						"<html><body>You were invited to following tournament: {{TournamentUrl}}.</body></html>"
				});
		}
	}
}
