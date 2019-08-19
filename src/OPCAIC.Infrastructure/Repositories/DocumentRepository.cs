using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using OPCAIC.Infrastructure.DbContexts;
using OPCAIC.Infrastructure.Dtos;
using OPCAIC.Infrastructure.Dtos.Documents;
using OPCAIC.Infrastructure.Entities;

namespace OPCAIC.Infrastructure.Repositories
{
	public class DocumentRepository : Repository<Document>, IDocumentRepository
	{
		/// <inheritdoc />
		public DocumentRepository(DataContext context, IMapper mapper) : base(context, mapper)
		{
		}

		/// <inheritdoc />
		public async Task<long> CreateAsync(NewDocumentDto document,
			CancellationToken cancellationToken)
		{
			var entity = new Document
			{
				Name = document.Name,
				Content = document.Content,
				TournamentId = document.TournamentId
			};

			DbSet.Add(entity);

			await Context.SaveChangesAsync(cancellationToken);

			return entity.Id;
		}

		/// <inheritdoc />
		public async Task<ListDto<DocumentDetailDto>> GetByFilterAsync(DocumentFilterDto filter,
			CancellationToken cancellationToken)
		{
			var query = DbSet.Filter(filter);

			return new ListDto<DocumentDetailDto>
			{
				List = await query
					.Skip(filter.Offset)
					.Take(filter.Count)
					.ProjectTo<DocumentDetailDto>(Mapper.ConfigurationProvider)
					.ToListAsync(cancellationToken),
				Total = await query.CountAsync(cancellationToken)
			};
		}

		/// <inheritdoc />
		public async Task<DocumentDetailDto> FindByIdAsync(long id,
			CancellationToken cancellationToken)
		{
			return await DbSet
				.Where(row => row.Id == id)
				.ProjectTo<DocumentDetailDto>(Mapper.ConfigurationProvider)
				.SingleOrDefaultAsync(cancellationToken);
		}

		/// <inheritdoc />
		public async Task<bool> UpdateAsync(long id, UpdateDocumentDto document,
			CancellationToken cancellationToken)
		{
			var entity = await DbSet.SingleOrDefaultAsync(row => row.Id == id, cancellationToken);
			if (entity == null)
			{
				return false;
			}

			entity.Name = document.Name;
			entity.Content = document.Content;
			entity.TournamentId = document.TournamentId;

			await Context.SaveChangesAsync(cancellationToken);
			return true;
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