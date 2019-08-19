using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OPCAIC.ApiService.Models.Tournaments;
using OPCAIC.ApiService.ModelValidationHandling.Attributes;
using OPCAIC.ApiService.Security;
using OPCAIC.ApiService.Services;
using System.Threading;
using System.Threading.Tasks;

namespace OPCAIC.ApiService.Controllers
{
	[ApiController]
	[Route("api/tournaments/{tournamentId}/participants")]
	public class TournamentParticipantsController : ControllerBase
	{
		private readonly ITournamentParticipantsService tournamentParticipantsService;

		public TournamentParticipantsController(ITournamentParticipantsService tournamentParticipantsService)
		{
			this.tournamentParticipantsService = tournamentParticipantsService;
		}

		/// <summary>
		///   Returns participants of tournament
		/// </summary>
		/// <returns>array of all tournaments</returns>
		[Authorize(RolePolicy.Organizer)]
		[HttpGet]
		[ProducesResponseType(typeof(TournamentParticipantPreview[]), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public Task<TournamentParticipantPreview[]> GetParticipantsAsync([ApiMinValue(1)] long tournamentId, CancellationToken cancellationToken)
		{
			return tournamentParticipantsService.GetParticipantsAsync(tournamentId, cancellationToken);
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
		public Task PostParticipantsAsync([ApiMinValue(1)] long tournamentId, [FromBody] NewTournamentParticipants model, CancellationToken cancellationToken)
		{
			return tournamentParticipantsService.CreateAsync(tournamentId, model.Emails, cancellationToken);
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
		public Task DeleteParticipantAsync([ApiMinValue(1)] long tournamentId, [ApiEmailAddress] string email, CancellationToken cancellationToken)
		{
			return tournamentParticipantsService.DeleteAsync(tournamentId, email, cancellationToken);
		}
	}
}
