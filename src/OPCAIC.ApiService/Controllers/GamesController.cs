using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OPCAIC.ApiService.Extensions;
using OPCAIC.ApiService.ModelBinding;
using OPCAIC.ApiService.Models;
using OPCAIC.ApiService.Security;
using OPCAIC.Application.Games.Commands;
using OPCAIC.Application.Games.Models;
using OPCAIC.Application.Games.Queries;
using OPCAIC.Application.Infrastructure;

namespace OPCAIC.ApiService.Controllers
{
	[Authorize]
	[Route("api/games")]
	public class GamesController : ControllerBase
	{
		private readonly IAuthorizationService authorizationService;
		private readonly IMediator mediator;

		public GamesController(IAuthorizationService authorizationService, IMediator mediator)
		{
			this.authorizationService = authorizationService;
			this.mediator = mediator;
		}

		/// <summary>
		///     Returns lists of games
		/// </summary>
		/// <returns>array of all games</returns>
		[HttpGet]
		[AllowAnonymous]
		[ProducesResponseType(typeof(PagedResult<GamePreviewDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[RequiresPermission(GamePermission.Search)]
		public Task<PagedResult<GamePreviewDto>> GetGamesAsync([FromQuery] GetGamesQuery filter,
			CancellationToken cancellationToken)
		{
			return mediator.Send(filter, cancellationToken);
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
		public async Task<IActionResult> PostAsync([FromBody] CreateGameCommand model,
			CancellationToken cancellationToken)
		{
			var id = await mediator.Send(model, cancellationToken);
			return CreatedAtRoute(nameof(GetGameByIdAsync), new {id}, new IdModel {Id = id});
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
		[AllowAnonymous]
		[ProducesResponseType(typeof(GameDetailDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<GameDetailDto> GetGameByIdAsync(long id,
			CancellationToken cancellationToken)
		{
			await authorizationService.CheckPermission(User, id, GamePermission.Read);
			return await mediator.Send(new GetGameQuery(id), cancellationToken);
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
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task UpdateAsync([FromRouteAndBody] UpdateGameCommand model,
			CancellationToken cancellationToken)
		{
			await authorizationService.CheckPermission(User, model.Id, GamePermission.Update);
			await mediator.Send(model, cancellationToken);
		}
	}
}