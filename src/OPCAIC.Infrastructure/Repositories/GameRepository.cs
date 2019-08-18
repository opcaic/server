using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using OPCAIC.Infrastructure.DbContexts;
using OPCAIC.Infrastructure.Dtos.Games;
using OPCAIC.Infrastructure.Entities;

namespace OPCAIC.Infrastructure.Repositories
{
	public class GameRepository
		: GenericRepository<Game, GameFilterDto, GamePreviewDto, GameDetailDto, NewGameDto,
				UpdateGameDto>,
			IGameRepository
	{
		/// <inheritdoc />
		public GameRepository(DataContext context, IMapper mapper)
			: base(context, mapper, QueryableExtensions.Filter)
		{
		}

		/// <inheritdoc />
		public Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken)
		{
			return ExistsByQueryAsync(g => g.Name == name, cancellationToken);
		}
	}
}