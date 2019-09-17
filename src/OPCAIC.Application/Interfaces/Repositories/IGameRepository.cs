using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Application.Dtos.Games;
using OPCAIC.Application.Specifications;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Application.Interfaces.Repositories
{
	public interface IGameRepository
		: IGenericRepository<GameFilterDto, GamePreviewDto, GameDetailDto, NewGameDto, UpdateGameDto>,
			IRepository<Game>
	{
		/// <summary>
		///     Checks whether game with given Id exists.
		/// </summary>
		/// <param name="name">Name of the game to be checked.</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken);

		/// <summary>
		///     Gets JSON schema for configurations of tournaments in given game.
		/// </summary>
		/// <param name="id">Id of the game.</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task<string> GetConfigurationSchemaAsync(long id, in CancellationToken cancellationToken);
	}
}