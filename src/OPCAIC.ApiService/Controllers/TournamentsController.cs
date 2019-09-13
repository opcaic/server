using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OPCAIC.ApiService.Extensions;
using OPCAIC.ApiService.Models;
using OPCAIC.ApiService.Models.Tournaments;
using OPCAIC.ApiService.Security;
using OPCAIC.ApiService.Services;
using OPCAIC.Services;

namespace OPCAIC.ApiService.Controllers
{
	[Authorize]
	[Route("api/tournaments")]
	public class TournamentsController : ControllerBase
	{
		private readonly IAuthorizationService authorizationService;
		private readonly IStorageService storage;
		private readonly ITournamentsService tournamentsService;

		public TournamentsController(ITournamentsService tournamentsService,
			IAuthorizationService authorizationService, IStorageService storage)
		{
			this.tournamentsService = tournamentsService;
			this.authorizationService = authorizationService;
			this.storage = storage;
		}

		/// <summary>
		///     Returns lists of tournaments
		/// </summary>
		/// <returns>array of all tournaments</returns>
		/// <response code="200">Tournament data found.</response>
		/// <response code="401">User is not authenticated.</response>
		/// <response code="403">User does not have permissions to this action.</response>
		[HttpGet]
		[ProducesResponseType(typeof(ListModel<TournamentPreviewModel>), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[RequiresPermission(TournamentPermission.Search)]
		public Task<ListModel<TournamentPreviewModel>> GetTournamentsAsync(
			[FromQuery] TournamentFilterModel filter, CancellationToken cancellationToken)
		{
			return tournamentsService.GetByFilterAsync(filter, cancellationToken);
		}

		/// <summary>
		///     Creates new tournament and returns its id
		/// </summary>
		/// <param name="model"></param>
		/// <param name="cancellationToken"></param>
		/// <response code="201">Tournament created</response>
		/// <response code="400">Data model is invalid.</response>
		[HttpPost]
		[ProducesResponseType(typeof(IdModel), StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status409Conflict)]
		[RequiresPermission(TournamentPermission.Create)]
		public async Task<IActionResult> PostAsync([FromBody] NewTournamentModel model,
			CancellationToken cancellationToken)
		{
			var id = await tournamentsService.CreateAsync(model, cancellationToken);
			return CreatedAtRoute(nameof(GetTournamentByIdAsync), new {id}, new IdModel {Id = id});
		}

		/// <summary>
		///     Gets tournament by id.
		/// </summary>
		/// <param name="id">Id of the tournament.</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		/// <response code="200">Tournament data found.</response>
		/// <response code="401">User is not authenticated.</response>
		/// <response code="403">User does not have permissions to this resource.</response>
		/// <response code="404">Resource was not found.</response>
		[HttpGet("{id}", Name = nameof(GetTournamentByIdAsync))]
		[ProducesResponseType(typeof(TournamentDetailModel), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<TournamentDetailModel> GetTournamentByIdAsync(long id,
			CancellationToken cancellationToken)
		{
			await authorizationService.CheckPermissions(User, id, TournamentPermission.Read);
			return await tournamentsService.GetByIdAsync(id, cancellationToken);
		}

		/// <summary>
		///     Updates tournament data by id.
		/// </summary>
		/// <param name="id">Id of the tournament.</param>
		/// <param name="model"></param>
		/// <param name="cancellationToken"></param>
		/// <response code="200">Tournament was successfully updated.</response>
		/// <response code="401">User is not authenticated.</response>
		/// <response code="403">User does not have permissions to this resource.</response>
		/// <response code="404">Resource was not found.</response>
		[HttpPut("{id}")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task UpdateAsync(long id, [FromBody] UpdateTournamentModel model,
			CancellationToken cancellationToken)
		{
			await authorizationService.CheckPermissions(User, id, TournamentPermission.Update);
			await tournamentsService.UpdateAsync(id, model, cancellationToken);
		}

		/// <summary>
		///     Downloads additional files needed for match execution and submission validation.
		/// </summary>
		/// <param name="id">Id of the tournament.</param>
		/// <response code="200">Additional files found.</response>
		/// <response code="401">User is not authenticated.</response>
		/// <response code="403">User does not have permissions to this action.</response>
		/// <response code="404">Tournament not found or has no additional files.</response>
		[HttpGet("{id}/files")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> DownloadAdditionalFiles(long id)
		{
			await authorizationService.CheckPermissions(User, id,
				TournamentPermission.DownloadAdditionalFiles);

			var archive = storage.ReadTournamentAdditionalFiles(id);
			if (archive == null)
			{
				return NoContent();
			}

			return File(archive, MediaTypeNames.Application.Zip, "tournamentFiles.zip");
		}

		/// <summary>
		///     Uploads additional files needed for match execution and submission validation.
		/// </summary>
		/// <param name="model">Archive containing the additional files.</param>
		/// <param name="cancellationToken"></param>
		/// <response code="200">Successfully uploaded.</response>
		/// <response code="400">Invalid file uploaded.</response>
		/// <response code="401">User is not authenticated.</response>
		/// <response code="403">User does not have permissions to this action.</response>
		/// <response code="404">Tournament not found.</response>
		[HttpPost("{id}/files")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task UploadAdditionalFiles(AdditionalTournamentFilesModel model,
			CancellationToken cancellationToken)
		{
			await authorizationService.CheckPermissions(User, model.TournamentId,
				TournamentPermission.UploadAdditionalFiles);

			using (var stream = storage.WriteTournamentAdditionalFiles(model.TournamentId, true))
			{
				await model.Archive.CopyToAsync(stream, cancellationToken);
			}
		}

		/// <summary>
		///     Manually starts evaluation of a tournament.
		/// </summary>
		/// <param name="id">Id of the tournament.</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		/// <response code="200">Successfully started.</response>
		/// <response code="400">Tournament already started.</response>
		/// <response code="401">User is not authenticated.</response>
		/// <response code="403">User does not have permissions to this action.</response>
		/// <response code="404">Tournament not found.</response>
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[HttpPost("{id}/start")]
		public async Task StartTournamentEvaluationAsync(long id,
			CancellationToken cancellationToken)
		{
			await authorizationService.CheckPermissions(User, id,
				TournamentPermission.StartTournamentEvaluation);

			await tournamentsService.StartTournamentEvaluation(id, cancellationToken);
		}
	}
}