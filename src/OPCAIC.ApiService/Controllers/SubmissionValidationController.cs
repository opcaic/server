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
		private readonly ISubmissionValidationRepository validationRepository;

		/// <inheritdoc />
		public ValidationController(IAuthorizationService authorizationService,
			ISubmissionValidationRepository validationRepository, IStorageService storage)
		{
			this.authorizationService = authorizationService;
			this.validationRepository = validationRepository;
			this.storage = storage;
		}

		/// <summary>
		///     Uploads zip archived results of a submission validation.
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
			if (Path.GetExtension(archive.FileName) != ".zip")
			{
				throw new BadRequestException(ValidationErrorCodes.UploadNotZip, null,
					nameof(archive));
			}

			await authorizationService.CheckPermissions(User, id,
				SubmissionValidationPermission.UploadResult);

			var storageDto = await validationRepository.FindStorageAsync(id, cancellationToken);
			using (var stream = storage.WriteSubmissionValidationResultArchive(storageDto))
			{
				await archive.CopyToAsync(stream, cancellationToken);
			}
		}
	}
}