using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OPCAIC.ApiService.Extensions;
using OPCAIC.ApiService.Models;
using OPCAIC.ApiService.Models.Games;
using OPCAIC.ApiService.Security;
using OPCAIC.ApiService.Services;

namespace OPCAIC.ApiService.Controllers
{
	[Route("api/games")]
	[Authorize]
	public class GamesController : ControllerBase
	{
		private readonly IGamesService gamesService;
		private readonly IAuthorizationService authorizationService;

		public GamesController(IGamesService gamesService, IAuthorizationService authorizationService)
		{
			this.gamesService = gamesService;
			this.authorizationService = authorizationService;
		}

		/// <summary>
		///     Returns lists of games
		/// </summary>
		/// <returns>array of all games</returns>
		[HttpGet]
		[ProducesResponseType(typeof(ListModel<GamePreviewModel>), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[RequiresPermission(GamePermission.Search)]
		public Task<ListModel<GamePreviewModel>> GetGamesAsync([FromQuery] GameFilterModel filter,
			CancellationToken cancellationToken)
		{
			return gamesService.GetByFilterAsync(filter, cancellationToken);
		}

		/// <summary>
		///     Creates new game and returns its id
		/// </summary>
		/// <param name="model"></param>
		/// <param name="cancellationToken"></param>
		/// <response code="201">Game created</response>
		/// <response code="400">Data model is invalid.</response>
		[HttpPost]
		[ProducesResponseType(typeof(IdModel), StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status409Conflict)]
		[RequiresPermission(GamePermission.Create)]
		public async Task<IActionResult> PostAsync([FromBody] NewGameModel model,
			CancellationToken cancellationToken)
		{
			var id = await gamesService.CreateAsync(model, cancellationToken);
			return CreatedAtRoute(nameof(GetGameByIdAsync), new IdModel {Id = id});
		}

		/// <summary>
		///     Gets game by id.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		/// <response code="200">Game data found.</response>
		/// <response code="401">User is not authenticated.</response>
		/// <response code="403">User does not have permissions to this resource.</response>
		/// <response code="404">Resource was not found.</response>
		[HttpGet("{id}", Name = nameof(GetGameByIdAsync))]
		[ProducesResponseType(typeof(GameDetailModel), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<GameDetailModel> GetGameByIdAsync(long id, CancellationToken cancellationToken)
		{
			await authorizationService.CheckPermissions(User, id, GamePermission.Read);
			return await gamesService.GetByIdAsync(id, cancellationToken);
		}

		/// <summary>
		///     Updates game data by id.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="model"></param>
		/// <param name="cancellationToken"></param>
		/// <response code="200">User was successfully updated.</response>
		/// <response code="401">User is not authenticated.</response>
		/// <response code="403">User does not have permissions to this resource.</response>
		/// <response code="404">Resource was not found.</response>
		[HttpPut("{id}")]
		[ProducesResponseType(typeof(GameDetailModel), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task UpdateAsync(long id, [FromBody] UpdateGameModel model,
			CancellationToken cancellationToken)
		{
			await authorizationService.CheckPermissions(User, id, GamePermission.Update);
			await gamesService.UpdateAsync(id, model, cancellationToken);
		}
	}
}