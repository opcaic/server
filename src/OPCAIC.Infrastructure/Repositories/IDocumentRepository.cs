using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Infrastructure.Dtos.Documents;
using OPCAIC.Infrastructure.Dtos.Tournaments;

namespace OPCAIC.Infrastructure.Repositories
{
	public interface IDocumentRepository
		: IGenericRepository<DocumentFilterDto, DocumentDetailDto, DocumentDetailDto, NewDocumentDto
				, UpdateDocumentDto>,
			IDeleteRepository,
			IAuthDataRepository<DocumentAuthDto>
	{
	}
}