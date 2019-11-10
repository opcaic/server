using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Application.Dtos;
using OPCAIC.Application.Dtos.MatchExecutions;
using OPCAIC.Application.MatchExecutions.Models;
using OPCAIC.Application.Specifications;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.Interfaces.Repositories
{
	public interface IMatchExecutionRepository
		: ICreateRepository<NewMatchExecutionDto>, ILookupRepository<MatchExecutionPreviewDto>, IRepository<MatchExecution>
	{
		/// <summary>
		///     Returns data needed to find where the archive with results of match execution with given id is stored.
		/// </summary>
		/// <param name="id">Id of the match execution.</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task<MatchExecutionStorageDto> FindExecutionForStorageAsync(long id,
			CancellationToken cancellationToken = default);

		Task<List<MatchExecutionRequestDataDto>> GetRequestsForSchedulingAsync(int count,
			WorkerJobState state, IEnumerable<string> gameKeys, CancellationToken cancellationToken);
	}
}