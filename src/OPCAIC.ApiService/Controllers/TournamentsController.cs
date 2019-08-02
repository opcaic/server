using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OPCAIC.ApiService.Exceptions;
using OPCAIC.ApiService.Security;
using OPCAIC.Infrastructure.DbContexts;
using OPCAIC.Infrastructure.Dtos;
using OPCAIC.Infrastructure.Entities;
using OPCAIC.Infrastructure.Repositories;

namespace OPCAIC.ApiService.Controllers
{
	[Route("api/tournaments")]
	public class TournamentsController : ControllerBase
	{
		private readonly ITournamentRepository tournamentRepository;

		public TournamentsController(ITournamentRepository tournamentRepository)
		{
			this.tournamentRepository = tournamentRepository;
		}


		/// <summary>
		///     Gets general information about all tournaments.
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[HttpGet]
		[Authorize(RolePolicy.User)]
		[ProducesResponseType(typeof(TournamentInfoDto), StatusCodes.Status200OK)]
		public Task<TournamentInfoDto[]> GetTournaments(CancellationToken cancellationToken)
		{
			return tournamentRepository.GetAllTournamentsInfo(cancellationToken);
		}

		/// <summary>
		///     Gets general information about the selected tournament.
		/// </summary>
		/// <param name="id">Id of the tournament</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[HttpGet("{id}")]
		[Authorize(RolePolicy.User)]
		[ProducesResponseType(typeof(TournamentInfoDto), StatusCodes.Status200OK)]
		public async Task<TournamentInfoDto> GetTournament(long id, CancellationToken cancellationToken)
		{
			var tournament = await tournamentRepository.GetAllTournamentInfo(id, cancellationToken);

			if (tournament == null)
			{
				throw new NotFoundException(nameof(Tournament), id);
			}

			return tournament;
		}

		/// <summary>
		///     Updates information about the tournament.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="tournament"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[HttpPut("{id}")]
		[Authorize(RolePolicy.Organizer)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public Task UpdateTournament(long id, TournamentInfoDto tournament, CancellationToken cancellationToken)
		{
			if (id != tournament.Id)
			{
				throw new BadRequestException("Invalid model of tournament.");
			}

			return tournamentRepository.UpdateTournament(tournament, cancellationToken);
		}
	}
}
