using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OPCAIC.Infrastructure.DbContexts;
using OPCAIC.Infrastructure.Dtos.Documents;
using OPCAIC.Infrastructure.Entities;

namespace OPCAIC.Infrastructure.Repositories
{
	public class DocumentRepository
		: GenericRepository<Document, DocumentFilterDto, DocumentDetailDto, DocumentDetailDto,
				NewDocumentDto, UpdateDocumentDto>,
			IDocumentRepository
	{
		/// <inheritdoc />
		public DocumentRepository(DataContext context, IMapper mapper)
			: base(context, mapper, QueryableExtensions.Filter)
		{
		}

		/// <inheritdoc />
		public Task<DocumentAuthDto> GetAuthorizationData(long id,
			CancellationToken cancellationToken = default)
		{
			return DbSet.Where(d => d.Id == id)
				.Select(d => new DocumentAuthDto
				{
					TournamentOwnerId = d.Tournament.OwnerId,
					TournamentManagersIds =
						d.Tournament.Managers.Select(m => m.UserId).ToArray()
				})
				.SingleOrDefaultAsync(cancellationToken);
		}
	}
}