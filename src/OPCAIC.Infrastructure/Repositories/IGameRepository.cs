﻿using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Infrastructure.Dtos.Games;

namespace OPCAIC.Infrastructure.Repositories
{
	public interface IGameRepository
		: IGenericRepository<GameFilterDto, GamePreviewDto, GameDetailDto, NewGameDto, UpdateGameDto
		>
	{
		/// <summary>
		///     Checks whether game with given Id exists.
		/// </summary>
		/// <param name="name">Name of the game to be checked.</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken);
	}
}