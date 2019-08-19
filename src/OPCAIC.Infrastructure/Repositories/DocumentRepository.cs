using AutoMapper;
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
		public async Task<bool> DeleteAsync(long id, CancellationToken cancellationToken)
		{
			var entity = await DbSet.SingleOrDefaultAsync(row => row.Id == id, cancellationToken);
			if (entity == null)
			{
				return false;
			}

			DbSet.Remove(entity);
			await Context.SaveChangesAsync(cancellationToken);
			return true;
		}
	}
}