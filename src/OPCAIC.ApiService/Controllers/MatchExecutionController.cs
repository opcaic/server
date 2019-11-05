using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MimeKit;
using OPCAIC.ApiService.Attributes;
using OPCAIC.ApiService.Extensions;
using OPCAIC.ApiService.Interfaces;
using OPCAIC.ApiService.Models;
using OPCAIC.ApiService.Security;
using OPCAIC.Application.Dtos.MatchExecutions;
using OPCAIC.Application.Infrastructure;
using OPCAIC.Application.Interfaces;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Application.Logging;
using OPCAIC.Application.MatchExecutions;
using OPCAIC.Application.MatchExecutions.Models;
using OPCAIC.Application.MatchExecutions.Queries;

namespace OPCAIC.ApiService.Controllers
{
	[Authorize]
	[Route("api/match-execution")]
	public class MatchExecutionController : ControllerBase
	{
		private readonly IAuthorizationService authorizationService;
		private readonly IMatchExecutionService executionService;
		private readonly ILogger<MatchExecutionController> logger;
		private readonly IMediator mediator;
		private readonly IMatchExecutionRepository repository;
		private readonly IStorageService storage;

		public MatchExecutionController(IStorageService storage,
			IMatchExecutionRepository repository, IAuthorizationService authorizationService,
			IMatchExecutionService executionService, ILogger<MatchExecutionController> logger,
			IMediator mediator)
		{
			this.storage = storage;
			this.repository = repository;
			this.authorizationService = authorizationService;
			this.executionService = executionService;
			this.logger = logger;
			this.mediator = mediator;
		}

		/// <summary>
		///     Uploads zip archived results of a given match execution.
		/// </summary>
		/// <param name="id">Id of the match execution.</param>
		/// <param name="model">Zip model with the results.</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[HttpPost("{id}/result")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[CustomRequestSizeLimit(CustomRequestSizeLimitAttribute.Type.Result)]
		public async Task UploadResult(long id, [FromForm] ResultArchiveModel model,
			CancellationToken cancellationToken)
		{
			await authorizationService.CheckPermissions(User, id,
				MatchExecutionPermission.UploadResult);

			var storageDto = await repository.FindExecutionForStorageAsync(id, cancellationToken);
			using (var stream = storage.WriteMatchResultArchive(storageDto))
			{
				await model.Archive.CopyToAsync(stream, cancellationToken);
			}

			logger.MatchExecutionResultUploaded(id);
		}

		/// <summary>
		///     Get filtered list of match executions.
		/// </summary>
		/// <param name="filter">Filter to use.</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		/// <response code="200">Match executions found.</response>
		/// <response code="401">User is not authenticated.</response>
		/// <response code="403">User does not have permissions to this action.</response>
		[HttpGet]
		[ProducesResponseType(typeof(PagedResult<MatchExecutionPreviewDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[RequiresPermission(MatchExecutionPermission.Search)]
		public async Task<PagedResult<MatchExecutionPreviewDto>> GetMatchExecutionsAsync(
			[FromQuery] GetMatchExecutionsQuery filter,
			CancellationToken cancellationToken)
		{
			return await mediator.Send(filter, cancellationToken);
		}


		/// <summary>
		///     Gets detailed information about the given match execution.
		/// </summary>
		/// <param name="id">Id of the match execution.</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[HttpGet("{id}")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<MatchExecutionDetailDto> GetByIdAsync(long id,
			CancellationToken cancellationToken)
		{
			await authorizationService.CheckPermissions(User, id,
				MatchExecutionPermission.ReadDetail);

			return await mediator.Send(new GetMatchExecutionQuery(id), cancellationToken);
		}

		/// <summary>
		///     Downloads match execution results as a zip model.
		/// </summary>
		/// <param name="id">Id of the match execution.</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[HttpGet("{id}/download")]
		[Produces(MediaTypeNames.Application.Zip)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> DownloadResult(long id,
			CancellationToken cancellationToken)
		{
			await authorizationService.CheckPermissions(User, id,
				MatchExecutionPermission.DownloadResults);

			var stream = await mediator.Send(new GetMatchResultArchiveQuery(id), cancellationToken);
			if (stream == null)
			{
				// no results yet
				return NotFound();
			}

			var filename = $"match-execution-{id}.zip";
			return File(stream, MimeTypes.GetMimeType(filename), filename);
		}

		/// <summary>
		///     Downloads match execution results as a zip model.
		/// </summary>
		/// <returns></returns>
		[HttpGet("{Id}/download/{Filename}")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> DownloadResult([FromRoute] GetMatchResultFileQuery query,
			CancellationToken cancellationToken)
		{
			await authorizationService.CheckPermissions(User, query.Id,
				MatchExecutionPermission.DownloadResults);

			var stream = await mediator.Send(query, cancellationToken);
			if (stream == null)
			{
				// no results yet or not found 
				return NotFound();
			}

			return File(stream, MimeTypes.GetMimeType(query.Filename), query.Filename);
		}
	}
}