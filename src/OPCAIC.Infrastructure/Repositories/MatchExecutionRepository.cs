using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using OPCAIC.Infrastructure.DbContexts;
using OPCAIC.Infrastructure.Dtos.MatchExecutions;
using OPCAIC.Infrastructure.Entities;

namespace OPCAIC.Infrastructure.Repositories
{
	public class MatchExecutionRepository
		: RepositoryBase<MatchExecution>, IMatchExecutionRepository
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

		/// <inheritdoc />
		public Task<long> CreateAsync(NewMatchExecutionDto dto, CancellationToken cancellationToken)
		{
			return CreateFromDtoAsync(dto, cancellationToken);
		}

		/// <inheritdoc />
		public Task<MatchExecutionAuthDto> GetAuthorizationData(long id, CancellationToken cancellationToken = default)
		{
			return GetDtoByIdAsync<MatchExecutionAuthDto>(id, cancellationToken);
		}
	}
}