using OPCAIC.Infrastructure.Dtos.Documents;

namespace OPCAIC.Infrastructure.Repositories
{
	public interface IDocumentRepository
		: ICreateRepository<NewDocumentDto>,
			ILookupRepository<DocumentFilterDto, DocumentDetailDto, DocumentDetailDto>,
			IUpdateRepository<UpdateDocumentDto>
	{
	}
}