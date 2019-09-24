using OPCAIC.Application.Documents.Queries;
using OPCAIC.Application.Dtos.Documents;
using OPCAIC.Application.Specifications;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Application.Interfaces.Repositories
{
	public interface IDocumentRepository
		: IGenericRepository<GetDocumentsQuery, DocumentDto, DocumentDto, NewDocumentDto
				, UpdateDocumentDto>,
			IDeleteRepository,
			IAuthDataRepository<DocumentAuthDto>,
			IRepository<Document>
	{
	}
}