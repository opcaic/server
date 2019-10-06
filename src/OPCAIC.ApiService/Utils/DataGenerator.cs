using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using AutoMapper;
using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OPCAIC.ApiService.Services;
using OPCAIC.Application.Dtos.MatchExecutions;
using OPCAIC.Application.Dtos.Submissions;
using OPCAIC.Application.Dtos.SubmissionValidations;
using OPCAIC.Application.Emails.Templates;
using OPCAIC.Application.Interfaces;
using OPCAIC.Application.Services;
using OPCAIC.Application.Services.MatchGeneration;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enumerations;
using OPCAIC.Domain.Enums;
using OPCAIC.Domain.ValueObjects;
using OPCAIC.Persistence;

namespace OPCAIC.ApiService.Utils
{
	public class DataGenerator
	{
		private static IMapper mapper;
		private static readonly Random random = new Random(0);
		private static readonly Faker faker = new Faker();

		private static void WriteEntry(ZipArchive archive, string entryName, string content)
		{
			using (var stream = new StreamWriter(archive.CreateEntry(entryName).Open()))
			{
				stream.Write(content);
			}
		}

		private static void EnsureSubmissionArchiveExists(IStorageService storage, Submission sub)
		{
			var storageDto = mapper.Map<SubmissionStorageDto>(sub);

			var archive = storage.ReadSubmissionArchive(storageDto);
			if (archive != null)
			{
				archive.Dispose();
				return; // already exists
			}

			// write something so that we have at least some file
			using (var zip = new ZipArchive(storage.WriteSubmissionArchive(storageDto),
				ZipArchiveMode.Create))
			{
				WriteEntry(zip, "input.txt", faker.Lorem.Paragraphs());
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

			using (var zip =
				new ZipArchive(storage.WriteSubmissionValidationResultArchive(storageDto),
					ZipArchiveMode.Create))
			{
				if (sub.CheckerResult != EntryPointResult.NotExecuted)
				{
					WriteEntry(zip, "check.0.stdout", faker.Lorem.Paragraph());
				}

				if (sub.CompilerResult != EntryPointResult.NotExecuted)
				{
					WriteEntry(zip, "compile.0.stdout", faker.Lorem.Paragraph());
				}

				if (sub.ValidatorResult != EntryPointResult.NotExecuted)
				{
					WriteEntry(zip, "validate.0.stdout", faker.Lorem.Paragraph());
				}
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

			using (var zip = new ZipArchive(storage.WriteMatchResultArchive(storageDto),
				ZipArchiveMode.Create))
			{
				for (int i = 0; i < execution.BotResults.Count; i++)
				{
					if (execution.BotResults[i].CompilerResult != EntryPointResult.NotExecuted)
					{
						WriteEntry(zip, $"compile.{i}.stdout", faker.Lorem.Paragraph());
					}
				}

				if (execution.ExecutorResult != EntryPointResult.NotExecuted)
				{
					WriteEntry(zip, "execute.stdout", faker.Lorem.Paragraphs());
				}
			}
		}

		public static void Initialize(IServiceProvider serviceProvider)
		{
			mapper = serviceProvider.GetRequiredService<IMapper>();
			using (var context = new DataContext(
				serviceProvider.GetRequiredService<DbContextOptions<DataContext>>()))
			{
				// dev settings: delete the database
				context.Database.EnsureDeleted();
				context.Database.EnsureCreated();

				// delete all existing files to prevent inconsistent state
				var conf = serviceProvider.GetRequiredService<IOptions<StorageConfiguration>>()
					.Value;
				if (Directory.Exists(conf.Directory))
				{
					Directory.Delete(conf.Directory, true);
				}

				// recreates directory structure
				var storage = serviceProvider.GetRequiredService<IStorageService>();

				// start with users
				var mgr = serviceProvider.GetRequiredService<UserManager>();
				var userAdmin = new User
				{
					FirstName = "Admin",
					LastName = "Opcaic",
					UserName = "admin",
					Role = UserRole.Admin,
					Email = "admin@opcaic.com",
					EmailConfirmed = true,
					LocalizationLanguage = "cs"
				};

				var userOrganizer = new User
				{
					FirstName = "Organizer",
					LastName = "Opcaic",
					UserName = "organizer",
					Role = UserRole.Organizer,
					Email = "organizer@opcaic.com",
					EmailConfirmed = true,
					LocalizationLanguage = "en"
				};

				var user = new User
				{
					FirstName = "User",
					LastName = "Opcaic",
					UserName = "user",
					Role = UserRole.User,
					Email = "user@opcaic.com",
					EmailConfirmed = true,
					LocalizationLanguage = "cs"
				};

				var userB = new User
				{
					FirstName = "User B",
					LastName = "Opcaic",
					UserName = "userB",
					Role = UserRole.User,
					Email = "userB@opcaic.com",
					EmailConfirmed = true,
					LocalizationLanguage = "cs"
				};

				mgr.CreateAsync(userAdmin, "Password").Wait();
				userAdmin = context.Users.Find(userAdmin.Id);
				mgr.CreateAsync(userOrganizer, "Password").Wait();
				userOrganizer = context.Users.Find(userOrganizer.Id);
				mgr.CreateAsync(user, "Password").Wait();
				user = context.Users.Find(user.Id);
				mgr.CreateAsync(userB, "Password").Wait();
				userB = context.Users.Find(userB.Id);

				FakeUsers(mgr);

				var users = context.Users.ToList();

				var gameChess = new Game
				{
					Name = "Chess",
					Key = "chess",
					Type = GameType.TwoPlayer,
					ImageUrl =
						"https://images.chesscomfiles.com/uploads/v1/article/17623.87bb05cd.668x375o.47d81802f1eb@2x.jpeg",
					DefaultTournamentImageOverlay = 0.7f,
					DefaultTournamentImageUrl =
						"https://images.chesscomfiles.com/uploads/v1/article/17623.87bb05cd.668x375o.47d81802f1eb@2x.jpeg",
					DefaultTournamentThemeColor = "#491e01",
					MaxAdditionalFilesSize = 1024 * 1024,
					ConfigurationSchema = "{}"
				};
				var game2048 = new Game
				{
					Name = "2048",
					Key = "2048",
					ImageUrl =
						"https://upload.wikimedia.org/wikipedia/commons/6/64/2048_Screenshot.png",
					DefaultTournamentImageOverlay = 0.7f,
					DefaultTournamentImageUrl =
						"https://upload.wikimedia.org/wikipedia/commons/6/64/2048_Screenshot.png",
					DefaultTournamentThemeColor = "#f67c5f",
					MaxAdditionalFilesSize = 1024 * 1024,
					ConfigurationSchema = "{}"
				};
				var gameDota = new Game
				{
					Name = "Dota 2",
					Key = "dota2",
					ImageUrl = "https://wallpapercave.com/wp/V8Ee1Bm.jpg",
					DefaultTournamentImageOverlay = 0.2f,
					DefaultTournamentImageUrl = "https://wallpapercave.com/wp/V8Ee1Bm.jpg",
					DefaultTournamentThemeColor = "#2d0f0a",
					MaxAdditionalFilesSize = 1024 * 1024,
					ConfigurationSchema = "{}"
				};
				var gameMario = new Game
				{
					Name = "Super Mario Bros.",
					Key = "mario",
					ImageUrl =
						"https://cdn02.nintendo-europe.com/media/images/10_share_images/games_15/nintendo_ds_22/SI_NDS_NewSuperMarioBrosDS_image1600w.jpg",
					DefaultTournamentImageOverlay = 0.6f,
					DefaultTournamentImageUrl =
						"https://cdn02.nintendo-europe.com/media/images/10_share_images/games_15/nintendo_ds_22/SI_NDS_NewSuperMarioBrosDS_image1600w.jpg",
					DefaultTournamentThemeColor = "#db0522",
					MaxAdditionalFilesSize = 1024 * 1024,
					ConfigurationSchema = "{}"
				};
				var gameSokoban = new Game
				{
					Name = "Sokoban",
					Key = "masokoban",
					ImageUrl =
						"https://raw.githubusercontent.com/kefik/Sokoban4J/master/Sokoban4J/screenshot2.png",
					DefaultTournamentImageOverlay = 0.6f,
					DefaultTournamentImageUrl =
						"https://raw.githubusercontent.com/kefik/Sokoban4J/master/Sokoban4J/screenshot2.png",
					DefaultTournamentThemeColor = "#97714a",
					MaxAdditionalFilesSize = 1024 * 1024,
					ConfigurationSchema = "{}"
				};

				context.Set<Game>().AddRange(gameChess, game2048, gameDota, gameMario, gameSokoban);
				context.SaveChanges();

				var tournamentChessElo = new Tournament
				{
					Owner = userAdmin,
					Name = "Chess ELO tournament",
					Game = gameChess,
					Format = TournamentFormat.Elo,
					MatchesPerDay = 50,
					RankingStrategy = TournamentRankingStrategy.Maximum,
					Scope = TournamentScope.Ongoing,
					Availability = TournamentAvailability.Private,
					EvaluationStarted = DateTime.Now.AddHours(-20),
					State = TournamentState.Running,
					Configuration = "{}",
					MaxSubmissionSize = 100 * 1024
				};
				var tournamentChessTable = new Tournament
				{
					Owner = userAdmin,
					Name = "Chess Table tournament",
					Game = gameChess,
					Format = TournamentFormat.Table,
					RankingStrategy = TournamentRankingStrategy.Maximum,
					Scope = TournamentScope.Deadline,
					Availability = TournamentAvailability.Public,
					State = TournamentState.Published,
					Configuration = "{}",
					MaxSubmissionSize = 100 * 1024
				};
				var tournamentChessDe = new Tournament
				{
					Owner = userAdmin,
					Name = "Chess Double-elimination tournament",
					Game = gameChess,
					Format = TournamentFormat.DoubleElimination,
					RankingStrategy = TournamentRankingStrategy.Maximum,
					Scope = TournamentScope.Deadline,
					Availability = TournamentAvailability.Public,
					State = TournamentState.Published,
					Configuration = "{}",
					MaxSubmissionSize = 100 * 1024
				};
				var tournament2048 = new Tournament
				{
					Owner = userAdmin,
					Name = "2048 single player",
					Game = game2048,
					Format = TournamentFormat.SinglePlayer,
					RankingStrategy = TournamentRankingStrategy.Maximum,
					Scope = TournamentScope.Ongoing,
					Availability = TournamentAvailability.Private,
					State = TournamentState.Published,
					Configuration = "{}",
					MaxSubmissionSize = 100 * 1024
				};
				var tournamentDotaSe = new Tournament
				{
					Owner = userAdmin,
					Name = "Summer Dota single elimination",
					Game = gameDota,
					Format = TournamentFormat.SingleElimination,
					RankingStrategy = TournamentRankingStrategy.Maximum,
					Scope = TournamentScope.Deadline,
					Deadline = DateTime.Now.AddDays(6),
					Availability = TournamentAvailability.Public,
					State = TournamentState.Finished,
					Configuration = "{}",
					MaxSubmissionSize = 100 * 1024
				};
				var tournamentDotaDe = new Tournament
				{
					Owner = userAdmin,
					Name = "Winter Dota double elimination",
					Game = gameDota,
					Format = TournamentFormat.DoubleElimination,
					RankingStrategy = TournamentRankingStrategy.Maximum,
					Scope = TournamentScope.Deadline,
					Deadline = DateTime.Now.AddDays(30),
					Availability = TournamentAvailability.Public,
					State = TournamentState.Published,
					Configuration = "{}",
					MaxSubmissionSize = 100 * 1024
				};
				var tournamentMario = new Tournament
				{
					Owner = userAdmin,
					Name = "Super Mario Bros.",
					Game = gameMario,
					Format = TournamentFormat.SinglePlayer,
					RankingStrategy = TournamentRankingStrategy.Maximum,
					Scope = TournamentScope.Deadline,
					Deadline = DateTime.Now.AddDays(17),
					Availability = TournamentAvailability.Public,
					State = TournamentState.Published,
					Configuration = "{}",
					MaxSubmissionSize = 100 * 1024,
					Description =
						"You can download our Java implementation of this game from the Git repository [MarioAI](https://github.com/medovina/MarioAI). If you need help importing it into Eclipse, see the excellent slides that Jakub Gemrot wrote explaining how to do this.\r\n\r\nThe game generates each level randomly, so your agent might succeed on some randomly generated levels and fail on others. I will evaluate your agent on a series of levels generated in several configurations:\r\n\r\n- LEVEL\\_0\\_FLAT: flat, empty terrain\r\n- LEVEL\\_1\\_JUMPING: terrain with hills, but no enemies\r\n- LEVEL\\_2\\_GOOMBAS: like LEVEL\\_1, but also with Goombas (a kind of monster)\r\n- LEVEL\\_3\\_TUBES: like LEVEL\\_2, but also with pipes with dangerous plants\r\n\r\nYour agent **succeeds** if it makes it to the end of each level, and fails otherwise. Its **success rate** is the fraction of randomly generated levels on which it succeeds.\r\n\r\nThis assignment is worth a total of 10 points. You can earn them as follows:\r\n- 2 points: 100% success rate on LEVEL_0_FLAT\r\n- 2 points: 95% success rate on LEVEL_1_JUMPING\r\n- 4 points: 75% success rate on LEVEL_2_GOOMBAS\r\n- 2 points: 75% success rate on LEVEL_3_TUBES\r\n\r\nDo not forget that Mario can both jump and shoot! :)\r\n\r\nFor the **tournament** for this assignment we will use LEVEL\\_4\\_SPIKIES, which has hills, Goombas, pipes and Spikies. The winner will be the agent with the highest success rate. I will break ties by choosing the agent with the fastest time to finish all levels.",
					MenuItems = 
						new List<MenuItem>
						{
							new ExternalUrlMenuItem()
							{
								Text = "Github",
								ExternalLink = "https://github.com/medovina/MarioAI"
							}
						}
				};
				var tournamentSokoban = new Tournament
				{
					Owner = userAdmin,
					Name = "Sokoban",
					Game = gameSokoban,
					Format = TournamentFormat.SinglePlayer,
					RankingStrategy = TournamentRankingStrategy.Maximum,
					Scope = TournamentScope.Deadline,
					Deadline = DateTime.Now.AddDays(17),
					Availability = TournamentAvailability.Public,
					State = TournamentState.Published,
					Configuration = "{}",
					MaxSubmissionSize = 100 * 1024,
					Description = "Sokoban is a type of puzzle video game, in which the player pushes crates or boxes around in a warehouse, trying to get them to storage locations. Sokoban was created in 1981 by Hiroyuki Imabayashi, and published in December 1982 by Thinking Rabbit, a software house based in Takarazuka, Japan.",
					MenuItems = 
						new List<MenuItem>
						{
							new DocumentLinkMenuItem { DocumentId = 4 },
							new DocumentLinkMenuItem { DocumentId = 5 },
							new DocumentLinkMenuItem { DocumentId = 6 },
							new ExternalUrlMenuItem
							{
								Text = "Github",
								ExternalLink = "https://github.com/medovina/Sokoban4J"
							}
						}
				};

				context.Set<Tournament>().AddRange(
					tournamentChessElo,
					tournamentChessTable,
					tournamentChessDe,
					tournament2048,
					tournamentDotaSe,
					tournamentDotaDe,
					tournamentMario,
					tournamentSokoban
				);

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
					},
					new Document
					{
						Name = "A* search (10 pts)",
						Tournament = tournamentSokoban,
						Content =
							"Implement A* search in Java. Write a general-purpose implementation that can search any problem that implements this interface:\r\n\r\n```java\r\n// S = state type, A = action type\r\npublic interface Problem<S, A> {\r\n    S initialState();\r\n    List<A> actions(S state);\r\n    S result(S state, A action);\r\n    boolean isGoal(S state);\r\n    int cost(S state, A action);\r\n\r\n    int estimate(S state);  // optimistic estimate of cost from state to goal\r\n}\r\n```\r\n\r\nProvide a search() method that returns a Node<S, A> representing the goal node that was found, or null if no goal was found. Node<S, A> should contain at least these fields:\r\n\r\n```java\r\nclass Node<S, A> {\r\n  public S state;\r\n  public Node<S, A> parent;  // parent node, or null if this is the start node\r\n  public A action;  // the action we took to get here from the parent\r\n  …\r\n}\r\n```\r\n\r\nYour code should live in a class that looks like this:\r\n```java\r\n// A* search\r\nclass AStar<S, A> {\r\n  public static <S, A> Node<S, A> search(Problem<S, A> prob, Stats stats) {\r\n    … your implementation here …\r\n  }     \r\n}\r\n```\r\nThe search should fill in the Stats object with statistics about the search. Stats should have at least this field:\r\n\r\n```java\r\npublic class Stats {\r\n    public int expanded;  // number of nodes expanded in the search\r\n}\r\n```\r\n\r\nYou may add any other statistics that you like.\r\n\r\nA caller will be able to invoke your search method like this:\r\n```java\r\n  Problem<Integer, Integer> p = new MyPuzzle();\r\n\r\n  Stats stats = new Stats();\r\n\r\n  Node<Integer, Integer> result = AStar.search(p, stats);\r\n  System.out.format(\"%d nodes were expanded\\n\", stats.expanded);\r\n```\r\nHere are some Problem instances that you can use for testing:\r\n- Cube.java\r\n- NPuzzle.java\r\n\r\nBe sure that your A* implementation returns optimal solutions to these test problems before you apply it to Sokoban.\r\n\r\n## Hints\r\nYou should be able to reuse virtually all of your uniform-cost search implementation from the previous assignment, because A* is essentially a variation of uniform-cost search.\r\n\r\nMake your Node class implement Comparable<Node<S, A>> and give it a compareTo method that compares nodes by f-cost, defined as f(n) = g(n) + h(n), where g(n) is the path cost and h(n) is the heuristic estimate.\r\n\r\nDo not compute the heuristic estimate on every call to compareTo; that will be expensive since the priority queue compares nodes very often. Instead, store the heuristic estimate as a field in the Node class so you can retrieve it immediately in compareTo."
					},
					new Document
					{
						Name = "Sokoban (10 pts)",
						Tournament = tournamentSokoban,
						Content =
							"Use A* search to solve Sokoban puzzles.\r\n\r\nDownload the Sokoban code from the Sokoban4J repository. Write your agent in the class cz.sokoban4j.playground.MyAgent. MyAgent.java which contains a simple depth-first search implementation that you can delete and replace with your own solver.\r\n\r\nIn your agent code, create an instance of the Problem<S, A> interface above and use an A* search to hunt for an optimal solution. Report the number of nodes that were expanded in the search by writing to standard output.\r\n\r\nYou wil want to use the Sokoban API.\r\n\r\nTo receive full credit for this assignment, implement the following:\r\n- A non-trivial admissible, consistent heuristic to guide the A* search. (The number of unplaced boxes is a trivial heuristic; surely you can do better than this.)\r\n- Dead square elimination. Your implementation should create a list of dead squares, which are squares from which a box cannot possibly be pushed to any goal. You should prune the search at any point where a box is pushed to a dead square.\r\n\r\nFor example, in the following picture every square that is adjacent to a wall is dead:\r\n\r\nYour agent should be able to solve all of the puzzles in the Sokoban4J/levels/Easy directory while expanding fewer than 10,000 nodes for each puzzle.\r\n\r\n## hints\r\n- The easiest approach is to use BoardCompact as your state class and either EDirection or CAction as your action class. If you want to make your solver more efficient, you could experiment with the other available board state classes (BoardSlim, BoardCompressed or StateMinimal) or even invent your own.\r\n- In your implementation of the result method, you will need to clone the given board state since result is supposed to leave the existing state unchanged. You can call perform to apply an action to the cloned state.\r\n- You will need to use a depth-first or breadth-first search to find dead squares. Use box targets as the starting points for your search, and search for squares that are alive.\r\n- To test your solver, run the main method in Evaluate.java , which will run your solver on a series of 80 Sokoban levels in increasing difficulty order. If you have a reasonable A* heuristic and dead square detection algorithm, then with the default 10-second time limit - you should be able to solve up to about the first 30 evaluation levels on a typical desktop or laptop computer, though the exact number will depend on your hardware of course.\r\n- For debugging your dead square detection, here is a snippet of code that will print the board and show which squares are dead. It assumes that dead is a 2-dimensional array of boolean indicating whether each square is dead:\r\n```java\r\nSystem.out.println(\"dead squares: \");\r\nfor (int y = 0 ; y < board.height() ; ++y) {\r\n    for (int x = 0 ; x < board.width() ; ++x)\r\n        System.out.print(CTile.isWall(board.tile(x, y)) ? '#' : (dead[x][y] ? 'X' : '_'));\r\n    System.out.println();\r\n}\r\n```\r\nFor example, on level 1 (Easy/level0001.s4jl) the output might look like this:\r\n\r\n```\r\n===== BOARD =====\r\n#####\r\n#p  #\r\n# a #\r\n#  1#\r\n#####\r\ndead squares: \r\n#####\r\n#XXX#\r\n#X__#\r\n#X__#\r\n#####\r\n```\r\n\r\n- There are many additional techniques and optimizations you could optionally implement. If you are interested in that, I recommend reading this Sokoban Solver documentation written by a former MFF student. It suggests various useful ideas (only some of which the author implemented in his own program). If those aren't enough for you, the Solver page on the Sokoban Wiki has many more ideas that could keep you busy for a while. :)"
					},
					new Document
					{
						Name = "Tournament",
						Tournament = tournamentSokoban,
						Content =
							"All entries received before the soft deadline will automatically be entered into a Sokoban tournament. In the tournament, I will run your solver on a series of Sokoban levels of increasing difficulty. Your solver will have 10 seconds to solve each level in a process with 2 Gb of RAM (specified with the -Xmx Java flag), on a machine with a 2.8 Ghz Intel CPU.\r\n\r\nAs soon as a program fails to solve 3 levels, its evaluation ends. Your solver's score will be the number of levels that it successfully solved before it reached the third unsolvable level.\r\n\r\nNote that solutions in the tournament need not be optimal! I will, however, resolve ties by choosing the solver that found a shorter solution.\r\n\r\nI will use the levels in Evaluate.java or a similar set of levels for the tournament. If you want to replicate the tournament conditions on your own machine, change the maxFail constant in main() in that file to 3 so the evalulation will continue until the 3rd unsolved level."
					});

				context.SaveChanges();

				var submissionChessAdmin = CreateSubmission(context, userAdmin, tournamentChessElo,
					Randomizer.Seed.NextDouble() * 1500);
				var submissionChessOrganizer = CreateSubmission(context, userOrganizer, tournamentChessElo,
					Randomizer.Seed.NextDouble() * 1500);
				context.SaveChanges();
				CreateSubmissions(context, users.OrderBy(u => u.Id).Skip(3).ToList(), tournamentChessElo);
				CreateSubmissions(context, users, tournamentChessTable);
				CreateSubmissions(context, users, tournamentChessDe);

				var matchChessAdminOrganizerSuccess = CreateMatch(context, tournamentChessElo, 1,
					submissionChessAdmin, submissionChessOrganizer);
				AddExecution(context, matchChessAdminOrganizerSuccess, EntryPointResult.Success,
					DateTime.Now);

				var matchChessAdminOrganizerQueued = CreateMatch(context, tournamentChessElo, 2,
					submissionChessAdmin, submissionChessOrganizer);
				AddExecution(context, matchChessAdminOrganizerQueued);

				var matchChessAdminOrganizerError = CreateMatch(context, tournamentChessElo, 3,
					submissionChessAdmin, submissionChessOrganizer);
				AddExecution(context, matchChessAdminOrganizerError, EntryPointResult.PlatformError,
					DateTime.Now.AddDays(-2));
				AddExecution(context, matchChessAdminOrganizerError, EntryPointResult.UserError,
					DateTime.Now.AddDays(-1));

				var submissionDotaAdmin = CreateSubmission(context, userAdmin, tournamentDotaSe, 0);
				var submissionDotaOrganizer = CreateSubmission(context, userOrganizer, tournamentDotaSe, 0);
				var submissionDotaUser = CreateSubmission(context, user, tournamentDotaSe, 0);
				var submissionDotaUserB = CreateSubmission(context, userB, tournamentDotaSe, 0);
				context.SaveChanges();

				var tree = MatchTreeGenerator.GenerateSingleElimination(4, false);
				var matchAdminOrganizer = CreateMatch(context, tournamentDotaSe, 0, submissionDotaAdmin,
					submissionDotaOrganizer);
				AddExecution(context, matchAdminOrganizer, EntryPointResult.Success, DateTime.Now.AddDays(-1));
				var matchUsers = CreateMatch(context, tournamentDotaSe, 1, submissionDotaUser,
					submissionDotaUserB);
				AddExecution(context, matchUsers, EntryPointResult.Success, DateTime.Now.AddDays(-2));
				var final = CreateMatch(context, tournamentDotaSe, 2, submissionDotaOrganizer,
					submissionDotaUserB);
				AddExecution(context, final, EntryPointResult.Success, DateTime.Now.AddDays(-3));

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

		private static void FakeUsers(UserManager manager)
		{
			Randomizer.Seed = new Random(100);

			var userFaker = new Faker<User>()
				.RuleFor(u => u.FirstName, (f, u) => f.Name.FirstName())
				.RuleFor(u => u.LastName, (f, u) => f.Name.LastName())
				.RuleFor(u => u.UserName, (f, u) => f.Internet.UserName(u.FirstName, u.LastName))
				.RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName))
				.RuleFor(u => u.EmailConfirmed, true)
				.RuleFor(u => u.Role, UserRole.User)
				.RuleFor(u => u.LocalizationLanguage, f => f.PickRandomParam(null, "en", "cs"));

			foreach (var user in userFaker.GenerateLazy(30))
			{
				manager.CreateAsync(user, "Password").GetAwaiter().GetResult();
			}
		}

		private static void CreateSubmissions(DataContext context,
			IEnumerable<User> users,
			Tournament tournament)
		{
			foreach (var user in users)
			{
				if (Randomizer.Seed.NextDouble() > 0.5)
				{
					// TO DO - submission score?
					// score?? lets say it's elo
					CreateSubmission(context, user, tournament,
						Randomizer.Seed.NextDouble() * 1500);
				}
			}

			context.SaveChanges();
		}

		private static Match CreateMatch(DataContext context, Tournament tournament, int index,
			params Submission[] participants)
		{
			var match = new Match
			{
				Tournament = tournament,
				Index = index,
				Participations =
					participants.Select(s => new SubmissionParticipation {Submission = s})
						.ToList(),
				Executions = new List<MatchExecution>() // executions to be added separately.
			};
			context.Add(match);
			context.SaveChanges();
			return match;
		}

		private static MatchExecution AddExecution(DataContext context, Match match,
			EntryPointResult executorResult = EntryPointResult.NotExecuted,
			DateTime? executed = null)
		{
			var i = 0;
			var matchExecution = new MatchExecution
			{
				JobId = Guid.NewGuid(),
				State = WorkerJobState.Finished,
				AdditionalData = "{ 'time': 10000 }",
				BotResults = match.Participations.Select(s => new SubmissionMatchResult
				{
					Submission = s.Submission,
					CompilerResult = EntryPointResult.Success,
					Score = i++,
					AdditionalData =
						$"{{ 'moves': {random.Next(1, 100)}, 'timePerMove': {random.Next(1, 100) / 3}, 'bonusPoints': {{ time: {random.Next(1, 1000)}, accuracy: {random.Next(1, 500)} }} }}"
				}).ToList(),
				ExecutorResult = executorResult,
				Executed = executed
			};
			match.Executions.Add(matchExecution);
			match.LastExecution = matchExecution;
			context.SaveChanges();
			return matchExecution;
		}

		private static Submission CreateSubmission(DataContext context, User author,
			Tournament tournament, double score)
		{
			if (tournament.Participants == null)
			{
				tournament.Participants = new List<TournamentParticipation>();

			}

			var participation = tournament.Participants.SingleOrDefault(p => p.UserId == author.Id);

			if (participation == null)
			{
				participation = new TournamentParticipation
				{
					Tournament = tournament,
					User = author,
					Submissions = new List<Submission>()
				};

				tournament.Participants.Add(participation);
			}

			context.SaveChanges();

			var submission = new Submission
			{
				Author = author,
				Tournament = tournament,
				Score = score,
				Validations = new List<SubmissionValidation>
				{
					new SubmissionValidation
					{
						JobId = Guid.NewGuid(),
						CheckerResult = EntryPointResult.Success,
						CompilerResult = EntryPointResult.Success,
						ValidatorResult = EntryPointResult.Success,
						State = WorkerJobState.Finished,
						Executed = DateTime.Now
					}
				}
			};
			participation.Submissions.Add(submission);
			participation.ActiveSubmission = submission;

			return submission;
		}

		private static void AddEmailTemplates(DataContext context)
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
		}
	}
}