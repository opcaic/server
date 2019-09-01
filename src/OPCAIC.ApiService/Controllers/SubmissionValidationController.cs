using System.IO;
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
using OPCAIC.Infrastructure.Repositories;
using OPCAIC.Services;

namespace OPCAIC.ApiService.Controllers
{
	[Authorize]
	[Route("api/validation")]
	public class ValidationController : ControllerBase
	{
		private readonly IAuthorizationService authorizationService;
		private readonly IStorageService storage;
		private readonly ISubmissionValidationRepository repository;

		/// <inheritdoc />
		public ValidationController(IAuthorizationService authorizationService,
			ISubmissionValidationRepository repository, IStorageService storage)
		{
			this.authorizationService = authorizationService;
			this.repository = repository;
			this.storage = storage;
		}

		/// <summary>
		///     Uploads zip archived results of a submission validation.
		/// </summary>
		/// <param name="id">Id of the submission validation.</param>
		/// <param name="archive">Zip archive with the results.</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[HttpPost("{id}/result")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task UploadResult(long id, [ApiRequired] IFormFile archive,
			CancellationToken cancellationToken)
		{
			await authorizationService.CheckPermissions(User, id,
				SubmissionValidationPermission.UploadResult);

			if (Path.GetExtension(archive.FileName) != ".zip")
			{
				throw new BadRequestException(ValidationErrorCodes.UploadNotZip, null,
					nameof(archive));
			}

			var storageDto = await repository.FindStorageAsync(id, cancellationToken);
			using (var stream = storage.WriteSubmissionValidationResultArchive(storageDto))
			{
				await archive.CopyToAsync(stream, cancellationToken);
			}
		}

		/// <summary>
		///     Downloads zip archived results of a submission validation.
		/// </summary>
		/// <param name="id">Id of the submission validation.</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[HttpGet("{id}/result")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> DownloadResult(long id,
			CancellationToken cancellationToken)
		{
			await authorizationService.CheckPermissions(User, id,
				SubmissionValidationPermission.DownloadResult);

			var storageDto = await repository.FindStorageAsync(id, cancellationToken);

			var stream = storage.ReadSubmissionValidationResultArchive(storageDto);
			if (stream == null)
			{
				// no results yet
				return NotFound();
			}

			return File(stream, Constants.GzipMimeType);
		}
	}
}