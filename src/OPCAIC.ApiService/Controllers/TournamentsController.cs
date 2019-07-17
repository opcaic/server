using Microsoft.AspNetCore.Mvc;

namespace OPCAIC.ApiService.Controllers
{
  using System.Collections.Generic;
  using System.Linq;
  using System.Net;
  using System.Threading;
  using System.Threading.Tasks;
  using Infrastructure.Entities;
  using Microsoft.AspNetCore.Cors;
  using Microsoft.EntityFrameworkCore;
    using OPCAIC.Infrastructure.DbContexts;
    using Security;

  [Route("api/tournaments")]
  [ApiController]
  public class TournamentsController : ControllerBase
  {
    private readonly DataContext context;

    public TournamentsController(DataContext context)
    {
      this.context = context;
    }


    [HttpGet]
    [ProducesResponseType(typeof(Tournament), (int)HttpStatusCode.OK)]
    public async Task<Tournament[]> GetTournaments()
    {
      return context.Set<Tournament>().ToArray();
    }

    [HttpGet("{id}")]
    public async Task<Tournament> GetTournament(int id, CancellationToken cancellationToken)
    {
      var tournament = await context.Set<Tournament>().SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

      if (tournament == null)
      {
        throw new NotFoundException(nameof(Tournament), id);
      }

      return tournament;
    }

    [HttpPut("{id}")]
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
