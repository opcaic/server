using OPCAIC.Infrastructure.Dtos.Documents;

namespace OPCAIC.Infrastructure.Repositories
{
	public interface IDocumentRepository
		: ICreateRepository<NewDocumentDto>,
			ILookupRepository<DocumentFilterDto, DocumentDetailDto, DocumentDetailDto>,
			IUpdateRepository<UpdateDocumentDto>
	{
		Task<long> CreateAsync(NewDocumentDto document, CancellationToken cancellationToken);

		Task<ListDto<DocumentDetailDto>> GetByFilterAsync(DocumentFilterDto filter,
			CancellationToken cancellationToken);

		Task<DocumentDetailDto> FindByIdAsync(long id, CancellationToken cancellationToken);

		Task<bool> UpdateAsync(long id, UpdateDocumentDto document,
			CancellationToken cancellationToken);
		
		Task<bool> DeleteAsync(long id, CancellationToken cancellationToken);
	}
}