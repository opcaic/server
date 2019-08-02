using System;
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
					new Game()
					{
						Name = "Chess",
						Created = DateTime.Now
					},
					new Game()
					{
						Name = "2048",
						Created = DateTime.Now
					},
					new Game()
					{
						Name = "Dota",
						Created = DateTime.Now
					},
					new Game()
					{
						Name = "Tic-Tao-Toe",
						Created = DateTime.Now
					}
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
					  Scope = TournamentScope.Ongoing,
				  },
				  new Tournament
				  {
					  Name = "2048 single player",
					  Game = context.Set<Game>().Single(x => x.Name == "2048"),
					  GameId = context.Set<Game>().Single(x => x.Name == "2048").Id,
					  Created = DateTime.Now,
					  Format = TournamentFormat.SinglePlayer,
					  RankingStrategy = TournamentRankingStrategy.Maximum,
					  Scope = TournamentScope.Ongoing,
				  },
				  new Tournament
				  {
					  Name = "Summer Dota single elimination",
					  Game = context.Set<Game>().Single(x => x.Name == "Dota"),
					  GameId = context.Set<Game>().Single(x => x.Name == "Dota").Id,
					  Created = DateTime.Now,
					  Format = TournamentFormat.SingleElimination,
					  RankingStrategy = TournamentRankingStrategy.Maximum,
					  Scope = TournamentScope.Deadline,
				  },
				  new Tournament
				  {
					  Name = "Ongoing Dota ELO",
					  Game = context.Set<Game>().Single(x => x.Name == "Dota"),
					  GameId = context.Set<Game>().Single(x => x.Name == "Dota").Id,
					  Created = DateTime.Now,
					  Format = TournamentFormat.Elo,
					  RankingStrategy = TournamentRankingStrategy.Maximum,
					  Scope = TournamentScope.Ongoing,
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
						EmailVerified = true
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
						EmailVerified = true
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
						EmailVerified = true
					});

				context.SaveChanges();
			}
		}
	}
}
