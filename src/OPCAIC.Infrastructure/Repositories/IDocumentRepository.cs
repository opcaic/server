using OPCAIC.Infrastructure.Dtos.Documents;

namespace OPCAIC.Infrastructure.Repositories
{
	public interface IDocumentRepository
		: IGenericRepository<DocumentFilterDto, DocumentDetailDto, DocumentDetailDto, NewDocumentDto
				, UpdateDocumentDto>,
			IDeleteRepository
	{
	}
}