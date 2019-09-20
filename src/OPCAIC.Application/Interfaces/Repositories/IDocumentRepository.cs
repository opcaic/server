using OPCAIC.Application.Dtos.Documents;
using OPCAIC.Application.Specifications;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Application.Interfaces.Repositories
{
	public interface IDocumentRepository
		: IGenericRepository<DocumentFilterDto, DocumentDetailDto, DocumentDetailDto, NewDocumentDto
				, UpdateDocumentDto>,
			IDeleteRepository,
			IAuthDataRepository<DocumentAuthDto>,
			IRepository<Document>
	{
	}
}