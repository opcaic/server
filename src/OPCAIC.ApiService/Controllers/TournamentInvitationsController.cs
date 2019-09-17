using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OPCAIC.ApiService.Models;
using OPCAIC.ApiService.Models.Tournaments;
using OPCAIC.ApiService.Security;
using OPCAIC.ApiService.Services;
using System.Threading;
using System.Threading.Tasks;
using OPCAIC.ApiService.Extensions;
using OPCAIC.ApiService.Interfaces;

namespace OPCAIC.ApiService.Controllers
{
	[Authorize]
	[Route("api/tournaments/{tournamentId}/participants")]
	public class TournamentInvitationsController : ControllerBase
	{
		private readonly IAuthorizationService authorizationService;
		private readonly ITournamentInvitationsService tournamentInvitationsService;

		public TournamentInvitationsController(ITournamentInvitationsService tournamentInvitationsService, IAuthorizationService authorizationService)
		{
			this.tournamentInvitationsService = tournamentInvitationsService;
			this.authorizationService = authorizationService;
		}

		/// <summary>
		///   Returns participants of tournament
		/// </summary>
		/// <returns>array of all tournaments</returns>
		[HttpGet]
		[ProducesResponseType(typeof(ListModel<TournamentInvitationPreviewModel>), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<ListModel<TournamentInvitationPreviewModel>> GetParticipantsAsync(long tournamentId, [FromQuery] TournamentInvitationFilter filter, CancellationToken cancellationToken)
		{
			await authorizationService.CheckPermissions(User, tournamentId, TournamentPermission.ManageInvites);
			return await tournamentInvitationsService.GetInvitationsAsync(tournamentId, filter, cancellationToken);
		}

		/// <summary>
		///		Adds new participants by email address to given tournament.
		/// </summary>
		/// <param name="tournamentId">id of tournament</param>
		/// <param name="model">participants</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[Authorize(RolePolicy.Organizer)]
		[HttpPost]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task PostParticipantsAsync(long tournamentId, [FromBody] NewTournamentInvitationsModel model, CancellationToken cancellationToken)
		{
			await authorizationService.CheckPermissions(User, tournamentId, TournamentPermission.ManageInvites);
			await tournamentInvitationsService.CreateAsync(tournamentId, model.Emails, cancellationToken);
		}

		/// <summary>
		/// Removes participant from tournament
		/// </summary>
		/// <param name="tournamentId">ID of tournament</param>
		/// <param name="model">Email of removed user</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[Authorize(RolePolicy.Organizer)]
		[HttpDelete("{email}")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task DeleteParticipantAsync(long tournamentId, DeleteTournamentInvitationModel model, CancellationToken cancellationToken)
		{
			await authorizationService.CheckPermissions(User, tournamentId, TournamentPermission.ManageInvites);
			await tournamentInvitationsService.DeleteAsync(tournamentId, model.Email, cancellationToken);
		}
	}
}
