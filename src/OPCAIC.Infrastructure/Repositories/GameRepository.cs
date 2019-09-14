using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OPCAIC.Infrastructure.DbContexts;
using OPCAIC.Infrastructure.Dtos.Games;
using OPCAIC.Infrastructure.Entities;
using OPCAIC.Infrastructure.Enums;

namespace OPCAIC.Infrastructure.Repositories
{
	public class GameRepository
		: GenericRepository<Game, GameFilterDto, GamePreviewDto, GameDetailDto, NewGameDto,
				UpdateGameDto>,
			IGameRepository
	{
		// must be kept in sync with TournamentRepository.ActiveTournamentPredicate
		public static readonly Expression<Func<Game, int>> ActiveTournamentsExpression
			= g => g.Tournaments.Count(t
				=> t.State == TournamentState.Published &&
				(t.Deadline == null || t.Deadline > DateTime.Now) ||
				t.State == TournamentState.Running &&
				t.Scope == TournamentScope.Ongoing);

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
		public Task<string> GetConfigurationSchemaAsync(long id,
			in CancellationToken cancellationToken)
		{
			return QueryById(id)
				.Select(g => g.ConfigurationSchema)
				.SingleOrDefaultAsync(cancellationToken);
		}
	}
}