using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OPCAIC.Application.Dtos.Games;
using OPCAIC.Application.Games.Queries;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;
using OPCAIC.Utils;

namespace OPCAIC.Persistence.Repositories
{
	public class GameRepository
		: GenericRepository<Game, GameDetailDto, NewGameDto, UpdateGameDto>,
			IGameRepository
	{
		/// <inheritdoc />
		public GameRepository(DataContext context, IMapper mapper)
			: base(context, mapper)
		{
		}

		/// <inheritdoc />
		public Task<bool> ExistsOtherByNameAsync(string name, long? currentGameId, CancellationToken cancellationToken)
		{
			return ExistsByQueryAsync(g => g.Name == name && (currentGameId == null || currentGameId.Value != g.Id), cancellationToken);
		}

		/// <inheritdoc />
		public Task<string> GetConfigurationSchemaAsync(long id,
			in CancellationToken cancellationToken)
		{
			return QueryById(id)
				.Select(g => g.ConfigurationSchema)
				.SingleOrDefaultAsync(cancellationToken);
		}
	}
}