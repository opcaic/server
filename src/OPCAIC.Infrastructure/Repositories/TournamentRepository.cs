using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using OPCAIC.Infrastructure.DbContexts;
using OPCAIC.Infrastructure.Dtos;
using OPCAIC.Infrastructure.Dtos.Tournaments;
using OPCAIC.Infrastructure.Entities;

namespace OPCAIC.Infrastructure.Repositories
{
	public class TournamentRepository : Repository<Tournament>, ITournamentRepository
	{
		/// <inheritdoc />
		public TournamentRepository(DataContext context, IMapper mapper) : base(context, mapper)
		{
		}

		public Task<bool> ExistsByIdAsync(long id, CancellationToken cancellationToken)
		{
			return DbSet.AnyAsync(row => row.Id == id, cancellationToken);
		}

		/// <inheritdoc />
		public async Task<long> CreateAsync(NewTournamentDto tournament,
			CancellationToken cancellationToken)
		{
			var entity = new Tournament
			{
				Name = tournament.Name,
				Description = tournament.Description,
				Format = tournament.Format,
				Scope = tournament.Scope,
				RankingStrategy = tournament.RankingStrategy,
				GameId = tournament.GameId
			};

			DbSet.Add(entity);

			await Context.SaveChangesAsync(cancellationToken);

			return entity.Id;
		}

		/// <inheritdoc />
		public async Task<ListDto<TournamentPreviewDto>> GetByFilterAsync(
			TournamentFilterDto filter,
			CancellationToken cancellationToken)
		{
			var query = DbSet.Filter(filter);

			return new ListDto<TournamentPreviewDto>
			{
				List = await query
					.Skip(filter.Offset)
					.Take(filter.Count)
					.ProjectTo<TournamentPreviewDto>(Mapper.ConfigurationProvider)
					.ToListAsync(cancellationToken),
				Total = await query.CountAsync(cancellationToken)
			};
		}

		/// <inheritdoc />
		public Task<TournamentDetailDto> FindByIdAsync(long id, CancellationToken cancellationToken)
		{
			return DbSet
				.Where(row => row.Id == id)
				.ProjectTo<TournamentDetailDto>(Mapper.ConfigurationProvider)
				.SingleOrDefaultAsync(cancellationToken);
		}

		/// <inheritdoc />
		public async Task<bool> UpdateAsync(long id, UpdateTournamentDto dto,
			CancellationToken cancellationToken)
		{
			var entity = await DbSet.SingleOrDefaultAsync(row => row.Id == id, cancellationToken);
			if (entity == null)
			{
				return false;
			}

			entity.Name = dto.Name;
			entity.Description = dto.Description;
			entity.GameId = dto.GameId;
			entity.Format = dto.Format;
			entity.Scope = dto.Scope;
			entity.RankingStrategy = dto.RankingStrategy;

			await Context.SaveChangesAsync(cancellationToken);
			return true;
		}
	}
}