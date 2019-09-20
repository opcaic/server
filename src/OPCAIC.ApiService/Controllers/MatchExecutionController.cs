using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OPCAIC.ApiService.Attributes;
using OPCAIC.ApiService.Extensions;
using OPCAIC.ApiService.Interfaces;
using OPCAIC.ApiService.Models;
using OPCAIC.ApiService.Models.Matches;
using OPCAIC.ApiService.Security;
using OPCAIC.ApiService.Services;
using OPCAIC.Application.Interfaces;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Application.Logging;

namespace OPCAIC.ApiService.Controllers
{
	[Authorize]
	[Route("api/match-execution")]
	public class MatchExecutionController : ControllerBase
	{
		private readonly IAuthorizationService authorizationService;
		private readonly IMatchExecutionRepository repository;
		private readonly IMatchExecutionService executionService;
		private readonly IStorageService storage;
		private readonly ILogger<MatchExecutionController> logger;

		public MatchExecutionController(IStorageService storage,
			IMatchExecutionRepository repository, IAuthorizationService authorizationService, IMatchExecutionService executionService, ILogger<MatchExecutionController> logger)
		{
			this.storage = storage;
			this.repository = repository;
			this.authorizationService = authorizationService;
			this.executionService = executionService;
			this.logger = logger;
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
			await authorizationService.CheckPermissions(User, id, MatchExecutionPermission.UploadResult);

			var storageDto = await repository.FindExecutionForStorageAsync(id, cancellationToken);
			using (var stream = storage.WriteMatchResultArchive(storageDto))
			{
				await model.Archive.CopyToAsync(stream, cancellationToken);
			}

			logger.MatchExecutionResultUploaded(id);
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
		public async Task<MatchExecutionDetailModel> GetByIdAsync(long id, CancellationToken cancellationToken)
		{
			await authorizationService.CheckPermissions(User, id,
				MatchExecutionPermission.ReadDetail);

			return await executionService.GetByIdAsync(id, cancellationToken);
		}

		/// <summary>
		///     Downloads match execution results as a zip model.
		/// </summary>
		/// <param name="id">Id of the match execution.</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[HttpGet("{id}/download")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> DownloadResult(int id, CancellationToken cancellationToken)
		{
			await authorizationService.CheckPermissions(User, id, MatchExecutionPermission.DownloadResults);

			var storageDto = await repository.FindExecutionForStorageAsync(id, cancellationToken);

			var stream = storage.ReadMatchResultArchive(storageDto);
			if (stream == null)
			{
				// no results yet
				return NotFound();
			}

			return File(stream, MediaTypeNames.Application.Zip);
		}
	}
}