using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using OPCAIC.Application.Dtos;
using OPCAIC.Application.Dtos.MatchExecutions;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Persistence.Repositories
{
	public class MatchExecutionRepository
		: EntityRepository<MatchExecution>, IMatchExecutionRepository
	{
		/// <inheritdoc />
		public MatchExecutionRepository(DataContext context, IMapper mapper) : base(context, mapper)
		{
		}

		/// <inheritdoc />
		public Task<MatchExecutionStorageDto> FindExecutionForStorageAsync(long id,
			CancellationToken cancellationToken = default)
		{
			return GetDtoByIdAsync<MatchExecutionStorageDto>(id, cancellationToken);
		}

		/// <inheritdoc />
		public Task<bool> UpdateFromJobAsync(Guid jobId, UpdateMatchExecutionDto dto,
			CancellationToken cancellationToken)
		{
			return UpdateFromDtoByQueryAsync(e => e.JobId == jobId, dto, cancellationToken);
		}

		public Task<bool> UpdateJobStateAsync(Guid jobId, JobStateUpdateDto dto,
			CancellationToken cancellationToken)
		{
			return UpdateFromDtoByQueryAsync(e => e.JobId == jobId, dto, cancellationToken);
		}

		/// <inheritdoc />
		public Task<List<MatchExecutionRequestDataDto>> GetRequestsForSchedulingAsync(int count,
			WorkerJobState state, IEnumerable<string> gameKeys, CancellationToken cancellationToken)
		{
			return DbSet.Where(e => e.State == state)
				.Where(e => gameKeys.Contains(e.Match.Tournament.Game.Key))
				.OrderBy(e => e.Created)
				.Take(count)
				.ProjectTo<MatchExecutionRequestDataDto>(Mapper.ConfigurationProvider)
				.ToListAsync(cancellationToken);
		}

		/// <inheritdoc />
		public Task<MatchExecutionRequestDataDto> GetRequestDataAsync(long id,
			CancellationToken cancellationToken)
		{
			return GetDtoByIdAsync<MatchExecutionRequestDataDto>(id, cancellationToken);
		}

		/// <inheritdoc />
		public Task<long> CreateAsync(NewMatchExecutionDto dto, CancellationToken cancellationToken)
		{
			return CreateFromDtoAsync(dto, cancellationToken);
		}

		/// <inheritdoc />
		public Task<MatchExecutionDto> FindByIdAsync(long id, CancellationToken cancellationToken)
		{
			return GetDtoByIdAsync<MatchExecutionDto>(id, cancellationToken);
		}
	}
}