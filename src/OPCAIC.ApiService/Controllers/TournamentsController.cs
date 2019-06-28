using Microsoft.AspNetCore.Mvc;

namespace OPCAIC.ApiService.Controllers
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Net;
	using System.Threading.Tasks;
	using Infrastructure.DbContexts;
	using Infrastructure.Entities;
	using Microsoft.AspNetCore.Cors;
	using Microsoft.EntityFrameworkCore;
	using Security;

	[Route("api/tournaments")]
	[ApiController]
	public class TournamentsController : ControllerBase
	{
		private readonly EntityFrameworkDbContext context;

		public TournamentsController(EntityFrameworkDbContext context)
		{
			this.context = context;
		}


		[HttpGet]
		[ProducesResponseType(typeof(Tournament), (int) HttpStatusCode.OK)]
		public async Task<ActionResult<Tournament[]>> GetTournaments()
		{
			return Ok(context.Tournaments.ToArray());
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<Tournament>> GetTournament(int id)
		{
			var tournament = context.Tournaments.SingleOrDefault(x => x.Id == id);

			if (tournament == null)
			{
				return NotFound();
			}

			return Ok(tournament);
		}

		[HttpPut("{id}")]
		public async Task<ActionResult<Tournament>> UpdateTournament(int id, Tournament tournament)
		{
			if (id != tournament.Id)
			{
				return BadRequest();
			}

			context.Entry(tournament).State = EntityState.Modified;
			context.SaveChanges();

			return NoContent();
		}
	}
}
