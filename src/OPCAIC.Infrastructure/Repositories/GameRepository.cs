using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
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

		/// <inheritdoc />
		public Task<string> GetConfigurationSchemaAsync(long id, in CancellationToken cancellationToken)
		{
			return QueryById(id)
				.Select(g => g.ConfigurationSchemaJson)
				.SingleOrDefaultAsync(cancellationToken);
		}
	}
}