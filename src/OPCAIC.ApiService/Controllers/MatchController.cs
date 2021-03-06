﻿using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OPCAIC.ApiService.Extensions;
using OPCAIC.ApiService.Security;
using OPCAIC.Application.Infrastructure;
using OPCAIC.Application.Matches.Command;
using OPCAIC.Application.Matches.Models;
using OPCAIC.Application.Matches.Queries;

namespace OPCAIC.ApiService.Controllers
{
	[Authorize]
	[Route("api/matches")]
	public class MatchController : ControllerBase
	{
		private readonly IAuthorizationService authorizationService;
		private readonly IMediator mediator;

		public MatchController(IAuthorizationService authorizationService, IMediator mediator)
		{
			this.authorizationService = authorizationService;
			this.mediator = mediator;
		}

		/// <summary>
		///     Get filtered list of matches.
		/// </summary>
		/// <param name="filter">Filter to use.</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		/// <response code="200">Matches found.</response>
		/// <response code="401">User is not authenticated.</response>
		/// <response code="403">User does not have permissions to this action.</response>
		[HttpGet]
		[AllowAnonymous]
		[ProducesResponseType(typeof(PagedResult<MatchDetailDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[RequiresPermission(MatchPermission.Search)]
		public Task<PagedResult<MatchPreviewDto>> GetMatchesAsync(GetMatchesQuery filter,
			CancellationToken cancellationToken)
		{
			return mediator.Send(filter, cancellationToken);
		}

		/// <summary>
		///     Gets match by id.
		/// </summary>
		/// <param name="id">Id of match to look for.</param>
		/// <param name="anonymize">Flag to override anonymization of the tournament</param>
		/// <param name="cancellationToken"></param>
		/// <response code="200">Match found.</response>
		/// <response code="401">User is not authenticated.</response>
		/// <response code="403">User does not have permissions to this action.</response>
		/// <response code="404">Match with given id does not exist.</response>
		[HttpGet("{id}")]
		[AllowAnonymous]
		[ProducesResponseType(typeof(MatchDetailDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<MatchDetailDto> GetMatchById(long id, [FromQuery] bool? anonymize, CancellationToken cancellationToken)
		{
			await authorizationService.CheckPermission(User, id, MatchPermission.Read);
			return await mediator.Send(new GetMatchQuery(id) { Anonymize = anonymize }, cancellationToken);
		}

		/// <summary>
		///     Gets match by id, including the information available only to tournament organizers.
		/// </summary>
		/// <param name="id">Id of match to look for.</param>
		/// <param name="anonymize">Flag to override anonymization of the tournament</param>
		/// <param name="cancellationToken"></param>
		/// <response code="200">Match found.</response>
		/// <response code="401">User is not authenticated.</response>
		/// <response code="403">User does not have permissions to this action.</response>
		/// <response code="404">Match with given id does not exist.</response>
		[HttpGet("{id}/admin")]
		[AllowAnonymous]
		[ProducesResponseType(typeof(MatchDetailDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<MatchAdminDto> GetMatchByIdForAdmin(long id, [FromQuery] bool? anonymize, CancellationToken cancellationToken)
		{
			await authorizationService.CheckPermission(User, id, MatchPermission.Read);
			return await mediator.Send(new GetMatchAdminQuery(id) { Anonymize = anonymize }, cancellationToken);
		}

		/// <summary>
		///     Manually queues another execution of a match.
		/// </summary>
		/// <param name="id">Id of the match.</param>
		/// <param name="cancellationToken"></param>
		/// <response code="200">Match found.</response>
		/// <response code="401">User is not authenticated.</response>
		/// <response code="403">User does not have permissions to this action.</response>
		/// <response code="404">Match with given id does not exist.</response>
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[HttpPost("{id}/execute")]
		public async Task ExecuteAsync(long id, CancellationToken cancellationToken)
		{
			await authorizationService.CheckPermission(User, id,
				MatchPermission.QueueMatchExecution);
			await mediator.Send(new ExecuteMatchCommand(id), cancellationToken);
		}
	}
}