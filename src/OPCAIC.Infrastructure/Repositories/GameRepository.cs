using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using OPCAIC.Infrastructure.DbContexts;
using OPCAIC.Infrastructure.Dtos;
using OPCAIC.Infrastructure.Dtos.Games;
using OPCAIC.Infrastructure.Entities;

namespace OPCAIC.Infrastructure.Repositories
{
	public class GameRepository : Repository<Game>, IGameRepository
	{
		/// <inheritdoc />
		public GameRepository(DataContext context, IMapper mapper) : base(context, mapper)
		{
		}

		/// <inheritdoc />
		public async Task<long> CreateAsync(NewGameDto game, CancellationToken cancellationToken)
		{
			var entity = new Game()
			{
				Name = game.Name,
			};

			DbSet.Add(entity);

			await Context.SaveChangesAsync(cancellationToken);

			return entity.Id;
		}

		/// <inheritdoc />
		public async Task<ListDto<GamePreviewDto>> GetByFilterAsync(GameFilterDto filter,
			CancellationToken cancellationToken)
		{
			var query = DbSet.Filter(filter);

			return new ListDto<GamePreviewDto>
			{
				List = await query
					.Skip(filter.Offset)
					.Take(filter.Count)
					.ProjectTo<GamePreviewDto>(Mapper.ConfigurationProvider)
					.ToListAsync(cancellationToken),
				Total = await query.CountAsync(cancellationToken)
			};
		}

		/// <inheritdoc />
		public Task<GameDetailDto> FindByIdAsync(long id, CancellationToken cancellationToken)
		{
			return DbSet
				.Where(row => row.Id == id)
				.ProjectTo<GameDetailDto>(Mapper.ConfigurationProvider)
				.SingleOrDefaultAsync(cancellationToken);
		}

		/// <inheritdoc />
		public async Task<bool> UpdateAsync(long id, UpdateGameDto dto,
			CancellationToken cancellationToken)
		{
			var entity = await DbSet.SingleOrDefaultAsync(row => row.Id == id, cancellationToken);
			if (entity == null)
				return false;

			entity.Name = dto.Name;

			await Context.SaveChangesAsync(cancellationToken);
			return true;
		}

		/// <inheritdoc />
		public Task<bool> ExistsByName(string name, CancellationToken cancellationToken)
		{
			return DbSet
				.Where(row => row.Name == name)
				.AnyAsync(cancellationToken);
		}
	}
}