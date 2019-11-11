using System.Collections.Generic;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OPCAIC.ApiService.Attributes;
using OPCAIC.ApiService.Extensions;
using OPCAIC.ApiService.ModelBinding;
using OPCAIC.ApiService.Models;
using OPCAIC.ApiService.Models.Tournaments;
using OPCAIC.ApiService.Security;
using OPCAIC.Application.Infrastructure;
using OPCAIC.Application.Interfaces;
using OPCAIC.Application.Tournaments.Command;
using OPCAIC.Application.Tournaments.Commands;
using OPCAIC.Application.Tournaments.Models;
using OPCAIC.Application.Tournaments.Queries;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using MimeKit;

namespace OPCAIC.ApiService.Controllers
{
	[Authorize]
	[Route("api/tournaments")]
	public class TournamentsController : ControllerBase
	{
		private readonly IAuthorizationService authorizationService;
		private readonly IMediator mediator;
		private readonly IStorageService storage;

		public TournamentsController(IAuthorizationService authorizationService,
			IStorageService storage, IMediator mediator)
		{
			this.authorizationService = authorizationService;
			this.storage = storage;
			this.mediator = mediator;
		}

		/// <summary>
		///     Returns lists of tournaments
		/// </summary>
		/// <returns>array of all tournaments</returns>
		/// <response code="200">Tournament data found.</response>
		/// <response code="401">User is not authenticated.</response>
		/// <response code="403">User does not have permissions to this action.</response>
		[HttpGet]
		[AllowAnonymous]
		[ProducesResponseType(typeof(PagedResult<TournamentPreviewDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[RequiresPermission(TournamentPermission.Search)]
		public Task<PagedResult<TournamentPreviewDto>> GetTournamentsAsync(
			[FromQuery] GetTournamentsQuery filter, CancellationToken cancellationToken)
		{
			return mediator.Send(filter, cancellationToken);
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
		public async Task<IActionResult> PostAsync([FromBody] CreateTournamentCommand model,
			CancellationToken cancellationToken)
		{
			var id = await mediator.Send(model, cancellationToken);
			return CreatedAtRoute(nameof(GetTournamentByIdAsync), new { id }, new IdModel { Id = id });
		}

		/// <summary>
		///     Copies a tournament with given id.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="cancellationToken"></param>
		/// <response code="201">Tournament created</response>
		/// <response code="400">Data model is invalid.</response>
		[HttpPost("{id}/clone")]
		[ProducesResponseType(typeof(IdModel), StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status409Conflict)]
		[RequiresPermission(TournamentPermission.Create)]
		public async Task<IActionResult> CloneAsync(long id, CancellationToken cancellationToken)
		{
			await authorizationService.CheckPermission(User, id, TournamentPermission.Read);
			var cloneId = await mediator.Send(new CloneTournamentCommand(id),
				cancellationToken);
			return CreatedAtRoute(nameof(GetTournamentByIdAsync), new { id = cloneId },
				new IdModel { Id = cloneId });
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
		[AllowAnonymous]
		[ProducesResponseType(typeof(TournamentDetailDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<TournamentDetailDto> GetTournamentByIdAsync(long id,
			CancellationToken cancellationToken)
		{
			await authorizationService.CheckPermission(User, id, TournamentPermission.Read);
			return await mediator.Send(new GetTournamentQuery(id), cancellationToken);
		}

		/// <summary>
		///     Updates tournament data by id.
		/// </summary>
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
		public async Task UpdateAsync([FromRouteAndBody] UpdateTournamentCommand model,
			CancellationToken cancellationToken)
		{
			await authorizationService.CheckPermission(User, model.Id,
				TournamentPermission.Update);
			await mediator.Send(model, cancellationToken);
		}

		/// <summary>
		///     Deletes tournament with given id.
		/// </summary>
		/// <param name="id">Id of the tournament.</param>
		/// <param name="cancellationToken"></param>
		/// <response code="200">Tournament was successfully deleted.</response>
		/// <response code="401">User is not authenticated.</response>
		/// <response code="403">User does not have permissions to this resource.</response>
		/// <response code="404">Resource was not found.</response>
		[HttpDelete("{id}")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task DeleteAsync(long id, CancellationToken cancellationToken)
		{
			await authorizationService.CheckPermission(User, id,
				TournamentPermission.Delete);

			await mediator.Send(new DeleteTournamentCommand(id), cancellationToken);
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
			await authorizationService.CheckPermission(User, id,
				TournamentPermission.DownloadAdditionalFiles);

			var archive = storage.ReadTournamentAdditionalFiles(id);
			if (archive == null)
			{
				return NoContent();
			}

			string filename = $"tournament-files-{id}.zip";
			return File(archive, MimeTypes.GetMimeType(filename), filename);
		}
		
		/// <summary>
		///     Deletes previously uploaded additional files for given tournament.
		/// </summary>
		/// <param name="id">Id of the tournament.</param>
		/// <response code="200">Additional files deleted.</response>
		/// <response code="401">User is not authenticated.</response>
		/// <response code="403">User does not have permissions to this action.</response>
		/// <response code="404">Tournament not found or has no additional files.</response>
		[HttpDelete("{id}/files")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> DeleteAdditionalFiles(long id)
		{
			await authorizationService.CheckPermission(User, id,
				TournamentPermission.ManageAdditionalFiles);

			storage.DeleteTournamentAdditionalFiles(id);
			return Ok();
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
		[CustomRequestSizeLimit(CustomRequestSizeLimitAttribute.Type.Tournament)]
		public async Task UploadAdditionalFiles(AdditionalTournamentFilesModel model,
			CancellationToken cancellationToken)
		{
			await authorizationService.CheckPermission(User, model.TournamentId,
				TournamentPermission.ManageAdditionalFiles);

			await using var stream = storage.WriteTournamentAdditionalFiles(model.TournamentId, true);
			await model.Archive.CopyToAsync(stream, cancellationToken);
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
			await authorizationService.CheckPermission(User, id,
				TournamentPermission.StartEvaluation);

			await mediator.Send(new StartTournamentEvaluationCommand { TournamentId = id },
				cancellationToken);
		}

		/// <summary>
		///     Manually pauses evaluation of a tournament.
		/// </summary>
		/// <param name="id">Id of the tournament.</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		/// <response code="200">Successfully paused.</response>
		/// <response code="400">Tournament is not being evaluated.</response>
		/// <response code="401">User is not authenticated.</response>
		/// <response code="403">User does not have permissions to this action.</response>
		/// <response code="404">Tournament not found.</response>
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[HttpPost("{id}/pause")]
		public async Task PauseTournamentEvaluationAsync(long id,
			CancellationToken cancellationToken)
		{
			await authorizationService.CheckPermission(User, id,
				TournamentPermission.PauseEvaluation);

			await mediator.Send(new PauseTournamentEvaluationCommand { TournamentId = id },
				cancellationToken);
		}

		/// <summary>
		///     Manually continues with the evaluation of a paused tournament.
		/// </summary>
		/// <param name="id">Id of the tournament.</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		/// <response code="200">Successfully unpaused.</response>
		/// <response code="400">Tournament is not paused.</response>
		/// <response code="401">User is not authenticated.</response>
		/// <response code="403">User does not have permissions to this action.</response>
		/// <response code="404">Tournament not found.</response>
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[HttpPost("{id}/unpause")]
		public async Task UnpauseTournamentEvaluationAsync(long id,
			CancellationToken cancellationToken)
		{
			await authorizationService.CheckPermission(User, id,
				TournamentPermission.UnpauseEvaluation);

			await mediator.Send(new UnpauseTournamentEvaluationCommand { TournamentId = id },
				cancellationToken);
		}

		/// <summary>
		///     Manually stops the evaluation of an ongoing tournament.
		/// </summary>
		/// <param name="id">Id of the tournament.</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		/// <response code="200">Successfully stopped.</response>
		/// <response code="400">Tournament is not being evaluated or is not ongoing.</response>
		/// <response code="401">User is not authenticated.</response>
		/// <response code="403">User does not have permissions to this action.</response>
		/// <response code="404">Tournament not found.</response>
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[HttpPost("{id}/stop")]
		public async Task StopTournamentEvaluationAsync(long id,
			CancellationToken cancellationToken)
		{
			await authorizationService.CheckPermission(User, id,
				TournamentPermission.StopEvaluation);

			await mediator.Send(new StopTournamentEvaluationCommand { TournamentId = id },
				cancellationToken);
		}


		/// <summary>
		///     Manually publishes tournament.
		/// </summary>
		/// <param name="id">Id of the tournament.</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		/// <response code="200">Successfully published.</response>
		/// <response code="400">Tournament is not in state "Created".</response>
		/// <response code="401">User is not authenticated.</response>
		/// <response code="403">User does not have permissions to this action.</response>
		/// <response code="404">Tournament not found.</response>
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[HttpPost("{id}/publish")]
		public async Task PublishTournamentEvaluationAsync(long id,
			CancellationToken cancellationToken)
		{
			await authorizationService.CheckPermission(User, id,
				TournamentPermission.Publish);

			await mediator.Send(new PublishTournamentCommand { TournamentId = id },
				cancellationToken);
		}

		/// <summary>
		///     Gets all managers of the given tournament.
		/// </summary>
		/// <param name="tournamentId"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[HttpGet("{tournamentId}/managers")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<List<string>> GetTournamentManagerAsync(long tournamentId,
			CancellationToken cancellationToken)
		{
			await authorizationService.CheckPermission(User, tournamentId,
				TournamentPermission.ReadManagers);

			return await mediator.Send(
				new GetTournamentManagersQuery(tournamentId),
				cancellationToken);
		}

		/// <summary>
		///     Adds a manager to a given tournament.
		/// </summary>
		/// <param name="tournamentId"></param>
		/// <param name="managerEmail"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[HttpPost("{tournamentId}/managers/{managerEmail}")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task AddTournamentManagerAsync(long tournamentId, string managerEmail,
			CancellationToken cancellationToken)
		{
			await authorizationService.CheckPermission(User, tournamentId,
				TournamentPermission.ManageManagers);

			await mediator.Send(
				new AddTournamentManagerCommand { Email = managerEmail, TournamentId = tournamentId },
				cancellationToken);
		}


		/// <summary>
		///     Deletes a manager of a given tournament.
		/// </summary>
		/// <param name="tournamentId"></param>
		/// <param name="managerEmail"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[HttpDelete("{tournamentId}/managers/{managerEmail}")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task DeleteTournamentManagerAsync(long tournamentId, string managerEmail,
			CancellationToken cancellationToken)
		{
			await authorizationService.CheckPermission(User, tournamentId,
				TournamentPermission.ManageManagers);

			await mediator.Send(
				new DeleteTournamentManagerCommand
				{
					Email = managerEmail, TournamentId = tournamentId
				}, cancellationToken);
		}

		/// <summary>
		///     Returns the leaderboard of a tournament.
		/// </summary>
		/// <returns>Leaderboard of tournament.</returns>
		/// <response code="200">Tournament data found.</response>
		/// <response code="401">User is not authenticated.</response>
		/// <response code="403">User does not have permissions to this resource.</response>
		/// <response code="404">Resource was not found.</response>
		[HttpGet("{id}/leaderboard")]
		[AllowAnonymous]
		[ProducesResponseType(typeof(TournamentLeaderboardDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<TournamentLeaderboardDto> GetLeaderboardByTournamentIdAsync(long id, [FromQuery] bool? anonymize,
			CancellationToken cancellationToken)
		{
			await authorizationService.CheckPermission(User, id, TournamentPermission.ViewLeaderboard);
			return await mediator.Send(new GetTournamentLeaderboardQuery(id) { Anonymize = anonymize }, cancellationToken);
		}
	}
}