using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OPCAIC.Infrastructure.Repositories;
using OPCAIC.Services;

namespace OPCAIC.ApiService.Controllers
{
	// TODO: Authorization for both users and workers
	[Route("api/files")]
	public class FileController : ControllerBase
	{
		public const string gzipMimeType = "application/x-zip-compressed";

		private const string SubmissionsRoute = "submissions";
		private const string MatchResultsRoute = "results";
		private readonly IMatchRepository matchRepository;

		private readonly IStorageService storage;
		private readonly ISubmissionRepository submissionRepository;

		public FileController(IStorageService storage, ISubmissionRepository submissionRepository,
			IMatchRepository matchRepository)
		{
			this.storage = storage;
			this.submissionRepository = submissionRepository;
			this.matchRepository = matchRepository;
		}

		/// <summary>
		///     Uploads a zip archive as as source code for the given submission.
		/// </summary>
		/// <param name="archive">The archive with source code for the submission.</param>
		/// <param name="id">Id of the submission</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[HttpPost(SubmissionsRoute + "/{id}")]
		[ProducesResponseType((int)HttpStatusCode.OK)]
		[ProducesResponseType((int)HttpStatusCode.Unauthorized)]
		[ProducesResponseType((int)HttpStatusCode.Forbidden)]
		[ProducesResponseType((int)HttpStatusCode.UnprocessableEntity)]
		public async Task<IActionResult> UploadSubmission(IFormFile archive, long id,
			CancellationToken cancellationToken)
		{
			var sub = await submissionRepository.FindSubmissionForStorageAsync(id,
				cancellationToken);
			if (sub == null || archive == null || Path.GetExtension(archive.FileName) != ".zip")
			{
				return UnprocessableEntity();
			}

			using (var stream = storage.WriteSubmissionArchive(sub))
			{
				await archive.CopyToAsync(stream, cancellationToken);
			}

			return Ok();
		}

		/// <summary>
		///     Downloads a zip archive containing source files for given submission.
		/// </summary>
		/// <param name="id">Id of the submission</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[HttpGet(SubmissionsRoute + "/{id}")]
		[ProducesResponseType((int)HttpStatusCode.OK)]
		[ProducesResponseType((int)HttpStatusCode.Unauthorized)]
		[ProducesResponseType((int)HttpStatusCode.Forbidden)]
		[ProducesResponseType((int)HttpStatusCode.NotFound)]
		public async Task<IActionResult> DownloadSubmission(long id,
			CancellationToken cancellationToken)
		{
			var sub = await submissionRepository.FindSubmissionForStorageAsync(id,
				cancellationToken);
			if (sub == null)
			{
				return NotFound();
			}

			var stream = storage.ReadSubmissionArchive(sub);
			if (stream == null)
			{
				return NotFound();
			}

			return File(stream, gzipMimeType);
		}

		/// <summary>
		///     Uploads zip archived results of a given match execution.
		/// </summary>
		/// <param name="archive">Zip archive with the results.</param>
		/// <param name="id">Id of the match execution.</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[HttpPost(MatchResultsRoute + "/{id}")]
		[ProducesResponseType((int)HttpStatusCode.OK)]
		[ProducesResponseType((int)HttpStatusCode.Unauthorized)]
		[ProducesResponseType((int)HttpStatusCode.Forbidden)]
		[ProducesResponseType((int)HttpStatusCode.UnprocessableEntity)]
		public async Task<IActionResult> UploadResult(IFormFile archive, long id,
			CancellationToken cancellationToken)
		{
			var me = await matchRepository.FindExecutionForStorageAsync(id, cancellationToken);
			if (me == null || archive == null || Path.GetExtension(archive.FileName) != ".zip")
			{
				return UnprocessableEntity();
			}

			using (var stream = storage.WriteMatchResultArchive(me))
			{
				await archive.CopyToAsync(stream, cancellationToken);
			}

			return Ok();
		}

		/// <summary>
		///     Downloads match execution results as a zip archive.
		/// </summary>
		/// <param name="id">Id of the match execution.</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[HttpGet(MatchResultsRoute + "/{id}")]
		[ProducesResponseType((int)HttpStatusCode.OK)]
		[ProducesResponseType((int)HttpStatusCode.Unauthorized)]
		[ProducesResponseType((int)HttpStatusCode.Forbidden)]
		[ProducesResponseType((int)HttpStatusCode.NotFound)]
		public async Task<IActionResult> DownloadResult(int id, CancellationToken cancellationToken)
		{
			var me = await matchRepository.FindExecutionForStorageAsync(id, cancellationToken);
			if (me == null)
			{
				return NotFound();
			}

			var stream = storage.ReadMatchResultArchive(me);
			if (stream == null)
			{
				return NotFound();
			}

			return File(stream, gzipMimeType);
		}
	}
}