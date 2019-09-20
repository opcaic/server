using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Application.Dtos.Games;
using OPCAIC.Application.Games.Models;
using OPCAIC.Application.Games.Queries;
using OPCAIC.Application.Specifications;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Application.Interfaces.Repositories
{
	public interface IGameRepository
		: IGenericRepository<GameFilterDto, GamePreviewModel, GameDetailDto, NewGameDto, UpdateGameDto>,
			IRepository<Game>
	{
		/// <summary>
		///     Checks whether another game with given name exists.
		/// </summary>
		/// <param name="name">Name of the game to be checked.</param>
		/// <param name="currentGameId">ID of currently updated game, which is excluded from search.</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task<bool> ExistsOtherByNameAsync(string name, long? currentGameId, CancellationToken cancellationToken);

		/// <summary>
		///     Gets JSON schema for configurations of tournaments in given game.
		/// </summary>
		/// <param name="id">Id of the game.</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task<string> GetConfigurationSchemaAsync(long id, in CancellationToken cancellationToken);
	}
}