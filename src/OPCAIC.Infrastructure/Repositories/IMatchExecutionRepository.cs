using System;
using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Infrastructure.Dtos.MatchExecutions;

namespace OPCAIC.Infrastructure.Repositories
{
	public interface IMatchExecutionRepository : ICreateRepository<NewMatchExecutionDto>
		, IAuthDataRepository<MatchExecutionAuthDto>
	{
		/// <summary>
		///     Returns data needed to find where the archive with results of match execution with given id is stored.
		/// </summary>
		/// <param name="id">Id of the match execution.</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task<MatchExecutionStorageDto> FindExecutionForStorageAsync(long id,
			CancellationToken cancellationToken = default);

		Task<bool> UpdateFromJobAsync(Guid jobId, UpdateMatchExecutionDto dto,
			CancellationToken cancellationToken);
	}
}