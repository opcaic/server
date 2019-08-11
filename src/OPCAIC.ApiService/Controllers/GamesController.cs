using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OPCAIC.ApiService.Models;
using OPCAIC.ApiService.Models.Games;
using OPCAIC.ApiService.Security;
using OPCAIC.ApiService.Services;

namespace OPCAIC.ApiService.Controllers
{
	[Route("api/games")]
	[ApiController]
	public class GamesController : ControllerBase
	{
		private readonly IGamesService gamesService;

		public GamesController(IGamesService gamesService) => this.gamesService = gamesService;

		/// <summary>
		///   Returns lists of games
		/// </summary>
		/// <returns>array of all games</returns>
		[Authorize(RolePolicy.Organizer)]
		[HttpGet(Name = nameof(GetGamesAsync))]
		[ProducesResponseType(typeof(ListModel<GamePreviewModel>), (int)HttpStatusCode.OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		public Task<ListModel<GamePreviewModel>> GetGamesAsync([FromQuery] GameFilterModel filter,
			CancellationToken cancellationToken)
			=> gamesService.GetByFilterAsync(filter, cancellationToken);

		/// <summary>
		///  Creates new game and returns its id
		/// </summary>
		/// <param name="model"></param>
		/// <param name="cancellationToken"></param>
		/// <response code="201">Game created</response>
		/// <response code="400">Data model is invalid.</response>
		[Authorize(RolePolicy.Admin)]
		[HttpPost]
		[ProducesResponseType(typeof(IdModel), StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status409Conflict)]
		public async Task<IActionResult> PostAsync([FromBody] NewGameModel model,
			CancellationToken cancellationToken)
		{
			var id = await gamesService.CreateAsync(model, cancellationToken);
			return CreatedAtRoute(nameof(GetGamesAsync), new IdModel {Id = id});
		}

		/// <summary>
		///		Gets game by id.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		/// <response code="200">Game data found.</response>
		/// <response code="401">User is not authenticated.</response>
		/// <response code="403">User does not have permissions to this resource.</response>
		/// <response code="404">Resource was not found.</response>
		[Authorize(RolePolicy.Admin)]
		[HttpGet("{id}")]
		[ProducesResponseType(typeof(GameDetailModel), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public Task<GameDetailModel> GetGameByIdAsync(long id, CancellationToken cancellationToken)
			=> gamesService.GetByIdAsync(id, cancellationToken);

		/// <summary>
		///		Updates game data by id.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="model"></param>
		/// <param name="cancellationToken"></param>
		/// <response code="200">User was successfully updated.</response>
		/// <response code="401">User is not authenticated.</response>
		/// <response code="403">User does not have permissions to this resource.</response>
		/// <response code="404">Resource was not found.</response>
		[Authorize(RolePolicy.Admin)]
		[HttpPut("{id}")]
		[ProducesResponseType(typeof(GameDetailModel), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public Task UpdateAsync(long id, [FromBody] UpdateGameModel model,
			CancellationToken cancellationToken)
			=> gamesService.UpdateAsync(id, model, cancellationToken);
	}
}
