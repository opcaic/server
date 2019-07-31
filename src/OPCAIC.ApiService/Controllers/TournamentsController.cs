using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Infrastructure.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using OPCAIC.Infrastructure.DbContexts;
using OPCAIC.ApiService.Security;
using OPCAIC.ApiService.Exceptions;

namespace OPCAIC.ApiService.Controllers
{
	[Route("api/tournaments")]
	public class TournamentsController : ControllerBase
	{
		private readonly DataContext context;

		public TournamentsController(DataContext context) => this.context = context;


		[HttpGet]
		[Authorize(RolePolicy.User)]
		[ProducesResponseType(typeof(Tournament), (int)HttpStatusCode.OK)]
		public async Task<Tournament[]> GetTournaments() => context.Set<Tournament>().ToArray();

		[HttpGet("{id}")]
		[Authorize(RolePolicy.User)]
		public async Task<Tournament> GetTournament(int id, CancellationToken cancellationToken)
		{
			var tournament = await context.Set<Tournament>()
				.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

			if (tournament == null)
			{
				throw new NotFoundException(nameof(Tournament), id);
			}

			return tournament;
		}

		[HttpPut("{id}")]
		[Authorize(RolePolicy.Organizer)]
		public async Task UpdateTournament(int id, Tournament tournament)
		{
			if (id != tournament.Id)
			{
				throw new BadRequestException("Invalid model of tournament.");
			}

			context.Entry(tournament).State = EntityState.Modified;
			context.SaveChanges();
		}
	}
}