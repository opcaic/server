using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Infrastructure.Dtos;
using OPCAIC.Infrastructure.Dtos.MatchExecutions;
using OPCAIC.Infrastructure.Enums;

namespace OPCAIC.Infrastructure.Repositories
{
	public interface IMatchExecutionRepository
		: ICreateRepository<NewMatchExecutionDto>, IAuthDataRepository<MatchExecutionAuthDto>, ILookupRepository<MatchExecutionDto>
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

		Task<bool> UpdateJobStateAsync(Guid jobId, JobStateUpdateDto dto,
			CancellationToken cancellationToken);

		Task<MatchExecutionRequestDataDto> GetRequestDataAsync(long id,
			CancellationToken cancellationToken);

		Task<List<MatchExecutionRequestDataDto>> GetRequestsForSchedulingAsync(int count,
			WorkerJobState state, IEnumerable<string> gameKeys, CancellationToken cancellationToken);
	}
}