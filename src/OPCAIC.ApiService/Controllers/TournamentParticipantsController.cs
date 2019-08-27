using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OPCAIC.ApiService.Models.Tournaments;
using OPCAIC.ApiService.ModelValidationHandling.Attributes;
using OPCAIC.ApiService.Security;
using OPCAIC.ApiService.Services;
using System.Threading;
using System.Threading.Tasks;
using OPCAIC.ApiService.Extensions;

namespace OPCAIC.ApiService.Controllers
{
	[Authorize]
	[Route("api/tournaments/{tournamentId}/participants")]
	public class TournamentParticipantsController : ControllerBase
	{
		private readonly IAuthorizationService authorizationService;
		private readonly ITournamentParticipantsService tournamentParticipantsService;

		public TournamentParticipantsController(ITournamentParticipantsService tournamentParticipantsService, IAuthorizationService authorizationService)
		{
			this.tournamentParticipantsService = tournamentParticipantsService;
			this.authorizationService = authorizationService;
		}

		/// <summary>
		///   Returns participants of tournament
		/// </summary>
		/// <returns>array of all tournaments</returns>
		[HttpGet]
		[ProducesResponseType(typeof(TournamentParticipantPreviewModel[]), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<TournamentParticipantPreviewModel[]> GetParticipantsAsync([ApiMinValue(1)] long tournamentId, CancellationToken cancellationToken)
		{
			await authorizationService.CheckPermissions(User, tournamentId, TournamentPermission.ManageInvites);
			return await tournamentParticipantsService.GetParticipantsAsync(tournamentId, cancellationToken);
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
		public async Task PostParticipantsAsync([ApiMinValue(1)] long tournamentId, [FromBody] NewTournamentParticipants model, CancellationToken cancellationToken)
		{
			await authorizationService.CheckPermissions(User, tournamentId, TournamentPermission.ManageInvites);
			await tournamentParticipantsService.CreateAsync(tournamentId, model.Emails, cancellationToken);
		}

		/// <summary>
		/// Removes participant from tournament
		/// </summary>
		/// <param name="tournamentId">ID of tournament</param>
		/// <param name="email">Email of removed user</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[Authorize(RolePolicy.Organizer)]
		[HttpDelete("{email}")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task DeleteParticipantAsync([ApiMinValue(1)] long tournamentId, [ApiEmailAddress] string email, CancellationToken cancellationToken)
		{
			await authorizationService.CheckPermissions(User, tournamentId, TournamentPermission.ManageInvites);
			await tournamentParticipantsService.DeleteAsync(tournamentId, email, cancellationToken);
		}
	}
}
