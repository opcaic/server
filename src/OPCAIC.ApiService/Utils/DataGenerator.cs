namespace OPCAIC.ApiService.Utils
{
	using System;
	using System.Linq;
	using Infrastructure.DbContexts;
	using Infrastructure.Entities;
	using Microsoft.EntityFrameworkCore;
	using Microsoft.Extensions.DependencyInjection;

	public class DataGenerator
	{
		public static void Initialize(IServiceProvider serviceProvider)
		{
			using (var context = new EntityFrameworkDbContext(
				serviceProvider.GetRequiredService<DbContextOptions<EntityFrameworkDbContext>>()))
			{
				// Look for any board games.
				if (context.Tournaments.Any())
				{
					return;   // Data was already seeded
				}

				context.Tournaments.AddRange(
					new Tournament()
					{
						Id = 1,
						Name = "Chess",
						Description = "Chess is a two-player strategy board game played on a chessboard, a checkered gameboard with 64 squares arranged in an 8×8 grid. The game is played by millions of people worldwide."
					},
					new Tournament()
					{
						Id = 2,
						Name = "2048",
						Description = "2048 is a single-player sliding block puzzle game designed by Italian web developer Gabriele Cirulli. The game's objective is to slide numbered tiles on a grid to combine them to create a tile with the number 2048."
					},
					new Tournament()
					{
						Id = 3,
						Name = "Dota",
						Description = "Dota 2 is a multiplayer online battle arena video game developed and published by Valve Corporation."
					},
					new Tournament()
					{
						Id = 4,
						Name = "Tic-Tao-Toe",
						Description = "Tic-Tao-Toe is a paper-and-pencil game for two players, X and O, who take turns marking the spaces in a 3×3 grid. The player who succeeds in placing three of their marks in a horizontal, vertical, or diagonal row wins the game."
					}
				);

				context.SaveChanges();
			}
		}
	}
}
