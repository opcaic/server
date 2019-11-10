using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using OPCAIC.Application.Dtos.BaseDtos;
using OPCAIC.Application.Dtos.MatchExecutions;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Application.MatchExecutions.Models;
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
		public Task<MatchExecutionDtoBase> FindExecutionForStorageAsync(long id,
			CancellationToken cancellationToken = default)
		{
			return GetDtoByIdAsync<MatchExecutionDtoBase>(id, cancellationToken);
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
		public Task<long> CreateAsync(NewMatchExecutionDto dto, CancellationToken cancellationToken)
		{
			return CreateFromDtoAsync(dto, cancellationToken);
		}

		/// <inheritdoc />
		public Task<MatchExecutionPreviewDto> FindByIdAsync(long id, CancellationToken cancellationToken)
		{
			return GetDtoByIdAsync<MatchExecutionPreviewDto>(id, cancellationToken);
		}
	}
}