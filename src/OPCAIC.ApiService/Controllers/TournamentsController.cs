using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OPCAIC.ApiService.Models;
using OPCAIC.ApiService.Models.Tournaments;
using OPCAIC.ApiService.Security;
using OPCAIC.ApiService.Services;

namespace OPCAIC.ApiService.Controllers
{
	[Route("api/tournaments")]
	public class TournamentsController : ControllerBase
	{
		private readonly ITournamentsService tournamentsService;

		public TournamentsController(ITournamentsService tournamentsService)
		{
			this.tournamentsService = tournamentsService;
		}

		/// <summary>
		///     Returns lists of tournaments
		/// </summary>
		/// <returns>array of all tournaments</returns>
		[Authorize(RolePolicy.Organizer)]
		[HttpGet]
		[ProducesResponseType(typeof(ListModel<TournamentPreviewModel>), (int)HttpStatusCode.OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		public Task<ListModel<TournamentPreviewModel>> GetTournamentsAsync(
			[FromQuery] TournamentFilterModel filter, CancellationToken cancellationToken)
		{
			return tournamentsService.GetByFilterAsync(filter, cancellationToken);
		}

		/// <summary>
		///     Creates new tournament and returns its id
		/// </summary>
		/// <param name="model"></param>
		/// <param name="cancellationToken"></param>
		/// <response code="201">Tournament created</response>
		/// <response code="400">Data model is invalid.</response>
		[Authorize(RolePolicy.Organizer)]
		[HttpPost]
		[ProducesResponseType(typeof(IdModel), StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status409Conflict)]
		public async Task<IActionResult> PostAsync([FromBody] NewTournamentModel model,
			CancellationToken cancellationToken)
		{
			var id = await tournamentsService.CreateAsync(model, cancellationToken);
			return CreatedAtRoute(nameof(GetTournamentByIdAsync), new IdModel {Id = id});
		}

		/// <summary>
		///     Gets tournament by id.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		/// <response code="200">Tournament data found.</response>
		/// <response code="401">User is not authenticated.</response>
		/// <response code="403">User does not have permissions to this resource.</response>
		/// <response code="404">Resource was not found.</response>
		[Authorize(RolePolicy.Organizer)]
		[HttpGet("{id}", Name = nameof(GetTournamentByIdAsync))]
		[ProducesResponseType(typeof(TournamentDetailModel), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public Task<TournamentDetailModel> GetTournamentByIdAsync(long id,
			CancellationToken cancellationToken)
		{
			return tournamentsService.GetByIdAsync(id, cancellationToken);
		}

		/// <summary>
		///     Updates tournament data by id.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="model"></param>
		/// <param name="cancellationToken"></param>
		/// <response code="200">Tournament was successfully updated.</response>
		/// <response code="401">User is not authenticated.</response>
		/// <response code="403">User does not have permissions to this resource.</response>
		/// <response code="404">Resource was not found.</response>
		[Authorize(RolePolicy.Organizer)]
		[HttpPut("{id}")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public Task UpdateAsync(long id, [FromBody] UpdateTournamentModel model,
			CancellationToken cancellationToken)
		{
			return tournamentsService.UpdateAsync(id, model, cancellationToken);
		}
	}
}