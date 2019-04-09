using Microsoft.AspNetCore.Mvc;

namespace OPCAIC.ApiService.Controllers
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Net;
	using System.Threading.Tasks;
	using Microsoft.AspNetCore.Cors;
	using Security;

	[Route("api/tournaments")]
	[ApiController]
	public class TournamentsController : ControllerBase
	{
		private readonly List<Tournament> tournaments;

		public TournamentsController()
		{
			tournaments = new List<Tournament>()
			{
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
				},
			};
		}


		[HttpGet]
		[ProducesResponseType(typeof(Tournament), (int) HttpStatusCode.OK)]
		public async Task<ActionResult<Tournament[]>> GetTournaments()
		{
			return Ok(tournaments.ToArray());
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<Tournament>> GetTournament(int id)
		{
			var tournament = tournaments.SingleOrDefault(x => x.Id == id);

			if (tournament == null)
			{
				return NotFound();
			}

			return Ok(tournament);
		}

		public class Tournament
		{
			public int Id { get; set; }

			public string Name { get; set; }

			public string Description { get; set; }
		}
	}
}
