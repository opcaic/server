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
using OPCAIC.ApiService.Models;
using OPCAIC.ApiService.Security;
using OPCAIC.Application.Infrastructure;
using OPCAIC.Application.Interfaces;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Application.Logging;
using OPCAIC.Application.SubmissionValidations.Models;
using OPCAIC.Application.SubmissionValidations.Queries;

namespace OPCAIC.ApiService.Controllers
{
	[Authorize]
	[Route("api/validation")]
	public class ValidationController : ControllerBase
	{
		private readonly IAuthorizationService authorizationService;
		private readonly ILogger<ValidationController> logger;
		private readonly IMediator mediator;
		private readonly ISubmissionValidationRepository repository;
		private readonly IStorageService storage;

		/// <inheritdoc />
		public ValidationController(IAuthorizationService authorizationService,
			ISubmissionValidationRepository repository, IStorageService storage, ILogger<ValidationController> logger,
			IMediator mediator)
		{
			this.authorizationService = authorizationService;
			this.repository = repository;
			this.storage = storage;
			this.logger = logger;
			this.mediator = mediator;
		}

		/// <summary>
		///     Uploads zip archived results of a submission validation.
		/// </summary>
		/// <param name="id">Id of the submission validation.</param>
		/// <param name="model">Zip archive with the results.</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[HttpPost("{id}/result")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[CustomRequestSizeLimit(CustomRequestSizeLimitAttribute.Type.Result)]
		public async Task UploadResult(long id, ResultArchiveModel model,
			CancellationToken cancellationToken)
		{
			await authorizationService.CheckPermission(User, id,
				SubmissionValidationPermission.UploadResult);

			var storageDto = await repository.FindStorageAsync(id, cancellationToken);
			await using (var stream = storage.WriteSubmissionValidationResultArchive(storageDto))
			{
				await model.Archive.CopyToAsync(stream, cancellationToken);
			}

			logger.SubmissionValidationResultUploaded(id);
		}

		/// <summary>
		///     Get filtered list of submission validations.
		/// </summary>
		/// <param name="filter">Filter to use.</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		/// <response code="200">Match executions found.</response>
		/// <response code="401">User is not authenticated.</response>
		/// <response code="403">User does not have permissions to this action.</response>
		[HttpGet]
		[ProducesResponseType(typeof(PagedResult<SubmissionValidationPreviewDto>),
			StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[RequiresPermission(SubmissionValidationPermission.Search)]
		public async Task<PagedResult<SubmissionValidationPreviewDto>> GetSubmissionValidationsAsync(
			[FromQuery] GetSubmissionValidationsQuery filter,
			CancellationToken cancellationToken)
		{
			return await mediator.Send(filter, cancellationToken);
		}


		/// <summary>
		///     Gets detailed information about the given submission validation.
		/// </summary>
		/// <param name="id">Id of the submission validation.</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[HttpGet("{id}")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<SubmissionValidationDetailDto> GetByIdAsync(long id,
			CancellationToken cancellationToken)
		{
			await authorizationService.CheckPermission(User, id,
				SubmissionValidationPermission.ReadDetail);

			return await mediator.Send(new GetSubmissionValidationQuery(id), cancellationToken);
		}

		/// <summary>
		///     Gets detailed information about the given submission validation, including the information that is available only to tournament organizers.
		/// </summary>
		/// <param name="id">Id of the submission validation.</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[HttpGet("{id}/admin")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<SubmissionValidationDetailDto> GetByIdForAdminAsync(long id,
			CancellationToken cancellationToken)
		{
			await authorizationService.CheckPermission(User, id,
				SubmissionValidationPermission.ReadAdmin);

			return await mediator.Send(new GetSubmissionValidationAdminQuery(id), cancellationToken);
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
			await authorizationService.CheckPermission(User, id,
				SubmissionValidationPermission.DownloadResult);

			var storageDto = await repository.FindStorageAsync(id, cancellationToken);

			var stream = storage.ReadSubmissionValidationResultArchive(storageDto);
			if (stream == null)
			{
				// no results yet
				return NotFound();
			}

			var filename = $"submission-validation-{id}.zip";
			return File(stream, MimeTypes.GetMimeType(filename), filename);
		}
	}
}