using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OPCAIC.ApiService.Models;
using OPCAIC.ApiService.Models.Tournaments;
using OPCAIC.ApiService.Security;
using OPCAIC.ApiService.Services;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using OPCAIC.ApiService.Extensions;
using OPCAIC.Application.Dtos.Tournaments;
using OPCAIC.Application.Infrastructure;
using OPCAIC.Application.TournamentInvitations.Commands;
using OPCAIC.Application.TournamentInvitations.Models;
using OPCAIC.Application.TournamentInvitations.Queries;

namespace OPCAIC.ApiService.Controllers
{
	[Authorize]
	[Route("api/tournaments/{tournamentId}/participants")]
	public class TournamentInvitationsController : ControllerBase
	{
		private readonly IMediator mediator;
		private readonly IAuthorizationService authorizationService;

		public TournamentInvitationsController(IMediator mediator, IAuthorizationService authorizationService)
		{
			this.authorizationService = authorizationService;
			this.mediator = mediator;
		}

		/// <summary>
		///   Returns participants of tournament
		/// </summary>
		/// <returns>array of all tournaments</returns>
		[HttpGet]
		[ProducesResponseType(typeof(PagedResult<TournamentInvitationDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<PagedResult<TournamentInvitationDto>> GetParticipantsAsync(long tournamentId, [FromQuery] GetTournamentInvitationsQuery filter, CancellationToken cancellationToken)
		{
			filter.TournamentId = tournamentId; // TODO: hide the TournamentId property from swagger
			await authorizationService.CheckPermission(User, tournamentId, TournamentPermission.ManageInvites);
			return await mediator.Send(filter, cancellationToken);
		}

		/// <summary>
		///		Adds new participants by email address to given tournament.
		/// </summary>
		/// <param name="tournamentId">Id of the tournament</param>
		/// <param name="model">participants</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[HttpPost]
		[Authorize(RolePolicy.Organizer)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task PostParticipantsAsync(long tournamentId, [FromBody] NewTournamentInvitationsModel model, CancellationToken cancellationToken)
		{
			await authorizationService.CheckPermission(User, tournamentId, TournamentPermission.ManageInvites);
			await mediator.Send(new InvitePlayersCommand
			{
				TournamentId = tournamentId,
				Emails = model.Emails,
				UserName = User.Identity.Name
			}, cancellationToken);
		}

		/// <summary>
		/// Removes participant from tournament
		/// </summary>
		/// <param name="tournamentId">Id of the tournament.</param>
		/// <param name="email">Email of the invitation to be deleted.</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[Authorize(RolePolicy.Organizer)]
		[HttpDelete("{email}")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task DeleteParticipantAsync(long tournamentId, string email, CancellationToken cancellationToken)
		{
			await authorizationService.CheckPermission(User, tournamentId, TournamentPermission.ManageInvites);
			await mediator.Send(new DeleteInvitationCommand(tournamentId, email), cancellationToken);
		}
	}
}
