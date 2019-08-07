using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OPCAIC.ApiService.Models;
using OPCAIC.ApiService.Models.Documents;
using OPCAIC.ApiService.Models.Tournaments;
using OPCAIC.ApiService.Security;
using OPCAIC.ApiService.Services;

namespace OPCAIC.ApiService.Controllers
{
	[Route("api/documents")]
	public class DocumentController : ControllerBase
	{
		private readonly IDocumentService documentService;

		public DocumentController(IDocumentService documentService)
			=> this.documentService = documentService;

		/// <summary>
		///     Get filtered list of documents.
		/// </summary>
		/// <param name="filter"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[Authorize(RolePolicy.Public)]
		[HttpGet(Name = nameof(GetDocumentsAsync))]
		[ProducesResponseType(typeof(ListModel<DocumentDetailModel>), (int)HttpStatusCode.OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		public Task<ListModel<DocumentDetailModel>> GetDocumentsAsync(
			DocumentFilterModel filter, CancellationToken cancellationToken)
			=> documentService.GetByFilterAsync(filter, cancellationToken);

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
		[Authorize(RolePolicy.Organizer)]
		[HttpPost]
		[ProducesResponseType(typeof(IdModel), StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status409Conflict)]
		public async Task<IActionResult> PostAsync([FromBody] NewDocumentModel model,
			CancellationToken cancellationToken)
		{
			var id = await documentService.CreateAsync(model, cancellationToken);
			return CreatedAtRoute(nameof(GetDocumentsAsync), new IdModel {Id = id});
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
		[Authorize(RolePolicy.Organizer)]
		[HttpGet("{id}")]
		[ProducesResponseType(typeof(DocumentDetailModel), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public Task<DocumentDetailModel> GetTournamentByIdAsync(long id,
			CancellationToken cancellationToken)
			=> documentService.GetByIdAsync(id, cancellationToken);

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
		[Authorize(RolePolicy.Organizer)]
		[HttpPut("{id}")]
		[ProducesResponseType(typeof(DocumentDetailModel), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public Task UpdateAsync(long id, [FromBody] UpdateDocumentModel model,
			CancellationToken cancellationToken)
			=> documentService.UpdateAsync(id, model, cancellationToken);
	}
}