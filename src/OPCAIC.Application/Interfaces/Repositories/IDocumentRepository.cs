using OPCAIC.Application.Dtos.Documents;

namespace OPCAIC.Application.Interfaces.Repositories
{
	public interface IDocumentRepository
		: IGenericRepository<DocumentFilterDto, DocumentDetailDto, DocumentDetailDto, NewDocumentDto
				, UpdateDocumentDto>,
			IDeleteRepository,
			IAuthDataRepository<DocumentAuthDto>
	{
	}
}