using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OPCAIC.ApiService.Exceptions;
using OPCAIC.ApiService.Extensions;
using OPCAIC.ApiService.Models;
using OPCAIC.ApiService.Models.Submissions;
using OPCAIC.ApiService.Models.Tournaments;
using OPCAIC.ApiService.Services;

namespace OPCAIC.ApiService.Controllers
{
	[Route("api/submissions")]
	[Authorize]
	public class SubmissionsController : ControllerBase
	{
		private readonly IAuthorizationService authorizationService;
		private readonly ILogger<SubmissionsController> logger;
		private readonly ISubmissionService submissionService;

		/// <inheritdoc />
		public SubmissionsController(ILogger<SubmissionsController> logger,
			IAuthorizationService authorizationService, ISubmissionService submissionService)
		{
			this.logger = logger;
			this.authorizationService = authorizationService;
			this.submissionService = submissionService;
		}

		/// <summary>
		///     Returns a list of submissions.
		/// </summary>
		/// <returns>A list of submissions filtered by given filter.</returns>
		/// <response code="200">Request ok.</response>
		/// <response code="400">Data model is invalid.</response>
		/// <response code="401">User is not authenticated.</response>
		/// <response code="403">User does not have permission to search submissions.</response>
		[HttpGet]
		[ProducesResponseType(typeof(ListModel<TournamentPreviewModel>), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		public Task<ListModel<SubmissionPreviewModel>> GetSubmissionsAsync(
			[FromQuery] SubmissionFilterModel filter, CancellationToken cancellationToken)
		{
			// TODO: authorize based on the filter
			return submissionService.GetByFilterAsync(filter, cancellationToken);
		}

		/// <summary>
		///     Creates new submission and returns its id
		/// </summary>
		/// <param name="model">Model data for the submission.</param>
		/// <param name="cancellationToken"></param>
		/// <response code="201">Submission created</response>
		/// <response code="400">Data model is invalid.</response>
		/// <response code="401">User is not authenticated.</response>
		/// <response code="403">User does not have permission to post given submission.</response>
		[HttpPost]
		[ProducesResponseType(typeof(IdModel), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		public async Task<IActionResult> PostAsync([FromForm] NewSubmissionModel model,
			CancellationToken cancellationToken)
		{
			// TODO: authorize creating submission for specified tournament (visibility etc.)
			var id = await submissionService.CreateAsync(model, User.GetId(), cancellationToken);
			// TODO: queue validation
			logger.SubmissionCreated(id, model);
			return CreatedAtRoute(nameof(GetSubmissionByIdAsync), new IdModel {Id = id});
		}

		/// <summary>
		///     Gets submission information by id.
		/// </summary>
		/// <param name="id">Id of the submission.</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		/// <response code="200">Submission data found.</response>
		/// <response code="401">User is not authenticated.</response>
		/// <response code="403">User does not have permissions to view given submission.</response>
		/// <response code="404">Resource was not found.</response>
		[HttpGet("{id}", Name = nameof(GetSubmissionByIdAsync))]
		[ProducesResponseType(typeof(SubmissionDetailModel), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public Task<SubmissionDetailModel> GetSubmissionByIdAsync(long id,
			CancellationToken cancellationToken)
		{
			// TODO: authorize 
			return submissionService.GetByIdAsync(id, cancellationToken);
		}

		/// <summary>
		///     Gets the submission archive for the given submission.
		/// </summary>
		/// <param name="id">Id of the submission.</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		/// <response code="200">Submission archive found.</response>
		/// <response code="401">User is not authenticated.</response>
		/// <response code="403">User does not have permissions to view given submission.</response>
		/// <response code="404">Resource was not found.</response>
		[HttpGet("{id}/download")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> GetSubmissionArchiveAsync(long id,
			CancellationToken cancellationToken)
		{
			// TODO: authorize user AND WORKERS
			return File(await submissionService.GetSubmissionArchiveAsync(id, cancellationToken), Constants.GzipMimeType);
		}

		/// <summary>
		///     Updates submission data by id.
		/// </summary>
		/// <param name="id">Id of the submission.</param>
		/// <param name="model">Data of the updated submission.</param>
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
		public Task UpdateAsync(long id, [FromBody] UpdateSubmissionModel model,
			CancellationToken cancellationToken)
		{
			// TODO: authorize
			logger.SubmissionUpdated(id, model);
			return submissionService.UpdateAsync(id, model, cancellationToken);
		}
	}
}