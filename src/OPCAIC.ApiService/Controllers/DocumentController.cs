using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OPCAIC.ApiService.Extensions;
using OPCAIC.ApiService.Models;
using OPCAIC.ApiService.Models.Documents;
using OPCAIC.ApiService.Security;
using OPCAIC.ApiService.Services;

namespace OPCAIC.ApiService.Controllers
{
	[Authorize]
	[Route("api/documents")]
	public class DocumentController : ControllerBase
	{
		private readonly IDocumentService documentService;
		private readonly IAuthorizationService authorizationService;

		public DocumentController(IDocumentService documentService, IAuthorizationService authorizationService)
		{
			this.documentService = documentService;
			this.authorizationService = authorizationService;
		}

		/// <summary>
		///     Get filtered list of documents.
		/// </summary>
		/// <param name="filter"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[HttpGet]
		[ProducesResponseType(typeof(ListModel<DocumentDetailModel>), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[RequiresPermission(DocumentPermission.Search)]
		public Task<ListModel<DocumentDetailModel>> GetDocumentsAsync(
			[FromQuery] DocumentFilterModel filter, CancellationToken cancellationToken)
		{
			return documentService.GetByFilterAsync(filter, cancellationToken);
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
		[RequiresPermission(DocumentPermission.Create)]
		public async Task<IActionResult> PostAsync([FromBody] NewDocumentModel model,
			CancellationToken cancellationToken)
		{
			await authorizationService.CheckPermissions(User, model.TournamentId, TournamentPermission.EditDocument);
			var id = await documentService.CreateAsync(model, cancellationToken);
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
		[ProducesResponseType(typeof(DocumentDetailModel), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<DocumentDetailModel> GetDocumentByIdAsync(long id,
			CancellationToken cancellationToken)
		{
			await authorizationService.CheckPermissions(User, id, DocumentPermission.Read);
			return await documentService.GetByIdAsync(id, cancellationToken);
		}

		/// <summary>
		///     Updates document data by id.
		/// </summary>
		/// <param name="id">Id of document to update.</param>
		/// <param name="model">New model of document.</param>
		/// <param name="cancellationToken"></param>
		/// <response code="200">Document was successfully updated.</response>
		/// <response code="401">User is not authenticated.</response>
		/// <response code="403">User does not have permissions to this resource.</response>
		/// <response code="404">Resource was not found.</response>
		[HttpPut("{id}")]
		[ProducesResponseType(typeof(DocumentDetailModel), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task UpdateAsync(long id, [FromBody] UpdateDocumentModel model,
			CancellationToken cancellationToken)
		{
			await authorizationService.CheckPermissions(User, id, DocumentPermission.Update);
			await authorizationService.CheckPermissions(User, model.TournamentId, TournamentPermission.EditDocument);
			await documentService.UpdateAsync(id, model, cancellationToken);
		}

		/// <summary>
		///     Deletes a document with given id.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[HttpDelete("{id}")]
		[ProducesResponseType(typeof(DocumentDetailModel), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task DeleteAsync(long id, CancellationToken cancellationToken)
		{
			await authorizationService.CheckPermissions(User, id, DocumentPermission.Delete);
			await documentService.DeleteAsync(id, cancellationToken);
		}
	}
}