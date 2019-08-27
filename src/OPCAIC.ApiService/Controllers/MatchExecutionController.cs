using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OPCAIC.ApiService.Exceptions;
using OPCAIC.ApiService.Extensions;
using OPCAIC.ApiService.ModelValidationHandling;
using OPCAIC.ApiService.ModelValidationHandling.Attributes;
using OPCAIC.ApiService.Security;
using OPCAIC.Infrastructure.Entities;
using OPCAIC.Infrastructure.Repositories;
using OPCAIC.Services;

namespace OPCAIC.ApiService.Controllers
{
	// TODO: Authorization for both users and workers
	[Authorize]
	[Route("api/match-execution")]
	public class MatchExecutionController : ControllerBase
	{
		private readonly IAuthorizationService authorizationService;
		private readonly IMatchRepository matchRepository;
		private readonly IStorageService storage;

		public MatchExecutionController(IStorageService storage,
			IMatchRepository matchRepository, IAuthorizationService authorizationService)
		{
			this.storage = storage;
			this.matchRepository = matchRepository;
			this.authorizationService = authorizationService;
		}

		/// <summary>
		///     Uploads zip archived results of a given match execution.
		/// </summary>
		/// <param name="id">Id of the match execution.</param>
		/// <param name="archive">Zip archive with the results.</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[HttpPost("{id}")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task UploadResult(long id, [ApiRequired] IFormFile archive,
			CancellationToken cancellationToken)
		{
			var storageDto = await matchRepository.FindExecutionForStorageAsync(id, cancellationToken);
			if (storageDto == null)
			{
				throw new NotFoundException(nameof(MatchExecution), id);
			}

			await authorizationService.CheckPermissions(User, id, MatchExecutionPermission.UploadResult);

			if (Path.GetExtension(archive.FileName) != ".zip")
			{
				throw new BadRequestException(ValidationErrorCodes.UploadNotZip, null, nameof(archive));
			}

			using (var stream = storage.WriteMatchResultArchive(storageDto))
			{
				await archive.CopyToAsync(stream, cancellationToken);
			}
		}

		/// <summary>
		///     Downloads match execution results as a zip archive.
		/// </summary>
		/// <param name="id">Id of the match execution.</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[HttpGet("{id}")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> DownloadResult(int id, CancellationToken cancellationToken)
		{
			var storageDto = await matchRepository.FindExecutionForStorageAsync(id, cancellationToken);
			if (storageDto == null)
			{
				return NotFound();
			}

			var stream = storage.ReadMatchResultArchive(storageDto);
			if (stream == null)
			{
				// no results yet
				return NotFound();
			}

			await authorizationService.CheckPermissions(User, id, MatchExecutionPermission.DownloadResults);

			return File(stream, Constants.GzipMimeType);
		}
	}
}