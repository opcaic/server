﻿using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OPCAIC.ApiService.Extensions;
using OPCAIC.ApiService.ModelBinding;
using OPCAIC.ApiService.Models;
using OPCAIC.ApiService.Security;
using OPCAIC.ApiService.Services;
using OPCAIC.Application.Documents.Commands;
using OPCAIC.Application.Documents.Models;
using OPCAIC.Application.Documents.Queries;
using OPCAIC.Application.Infrastructure;

namespace OPCAIC.ApiService.Controllers
{
	[Authorize]
	[Route("api/documents")]
	public class DocumentController : ControllerBase
	{
		private readonly IAuthorizationService authorizationService;
		private readonly IMediator mediator;

		public DocumentController(IAuthorizationService authorizationService, IMediator mediator)
		{
			this.authorizationService = authorizationService;
			this.mediator = mediator;
		}

		/// <summary>
		///     Get filtered list of documents.
		/// </summary>
		/// <param name="filter"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[HttpGet]
		[AllowAnonymous]
		[ProducesResponseType(typeof(PagedResult<DocumentDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[RequiresPermission(DocumentPermission.Search)]
		public Task<PagedResult<DocumentDto>> GetDocumentsAsync(
			[FromQuery] GetDocumentsQuery filter, CancellationToken cancellationToken)
		{
			return mediator.Send(filter, cancellationToken);
		}

		/// <summary>
		///     Creates new document and returns its id
		/// </summary>
		/// <param name="model">New document to create.</param>
		/// <param name="cancellationToken"></param>
		/// <response code="201">Document created</response>
		/// <response code="400">Data model is invalid.</response>
		/// <response code="401">User is not authenticated.</response>
		/// <response code="403">User does not have permissions to this resource.</response>
		/// <response code="409">Conflict in posting.</response>
		[HttpPost]
		[ProducesResponseType(typeof(IdModel), StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status409Conflict)]
		public async Task<IActionResult> PostAsync([FromBody] CreateDocumentCommand model,
			CancellationToken cancellationToken)
		{
			await authorizationService.CheckPermission(User, model.TournamentId, TournamentPermission.ManageDocuments);
			var id = await mediator.Send(model, cancellationToken);
			return CreatedAtRoute(nameof(GetDocumentByIdAsync), new { id }, new IdModel {Id = id});
		}

		/// <summary>
		///     Gets document by id.
		/// </summary>
		/// <param name="id">Id of the searched document.</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		/// <response code="200">Document data found.</response>
		/// <response code="401">User is not authenticated.</response>
		/// <response code="403">User does not have permissions to this resource.</response>
		/// <response code="404">Resource was not found.</response>
		[HttpGet("{id}", Name = nameof(GetDocumentByIdAsync))]
		[AllowAnonymous]
		[ProducesResponseType(typeof(DocumentDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<DocumentDto> GetDocumentByIdAsync(long id,
			CancellationToken cancellationToken)
		{
			await authorizationService.CheckPermission(User, id, DocumentPermission.Read);
			return await mediator.Send(new GetDocumentQuery(id), cancellationToken);
		}

		/// <summary>
		///     Updates document data by id.
		/// </summary>
		/// <param name="model">New model of document.</param>
		/// <param name="cancellationToken"></param>
		/// <response code="200">Document was successfully updated.</response>
		/// <response code="401">User is not authenticated.</response>
		/// <response code="403">User does not have permissions to this resource.</response>
		/// <response code="404">Resource was not found.</response>
		[HttpPut("{id}")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task UpdateAsync([FromRouteAndBody] UpdateDocumentCommand model,
			CancellationToken cancellationToken)
		{
			await authorizationService.CheckPermission(User, model.Id, DocumentPermission.Update);
			await mediator.Send(model, cancellationToken);
		}

		/// <summary>
		///     Deletes a document with given id.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[HttpDelete("{id}")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task DeleteAsync(long id, CancellationToken cancellationToken)
		{
			await authorizationService.CheckPermission(User, id, DocumentPermission.Delete);
			await mediator.Send(new DeleteDocumentCommand(id), cancellationToken);
		}
	}
}