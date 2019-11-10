using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using OPCAIC.Application.Dtos;
using OPCAIC.Application.Dtos.SubmissionValidations;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Application.SubmissionValidations.Models;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Persistence.Repositories
{
	public class SubmissionValidationRepository
		: EntityRepository<SubmissionValidation>, ISubmissionValidationRepository
	{
		/// <inheritdoc />
		public SubmissionValidationRepository(DataContext context, IMapper mapper) 
			: base(context, mapper)
		{
		}

		/// <inheritdoc />
		public Task<long> CreateAsync(NewSubmissionValidationDto dto,
			CancellationToken cancellationToken)
		{
			return CreateFromDtoAsync(dto, cancellationToken);
		}

		/// <inheritdoc />
		public Task<SubmissionValidationStorageDto> FindStorageAsync(long id,
			CancellationToken cancellationToken)
		{
			return GetDtoByIdAsync<SubmissionValidationStorageDto>(id, cancellationToken);
		}

		public Task<bool> UpdateJobStateAsync(Guid jobId, JobStateUpdateDto dto,
			CancellationToken cancellationToken)
		{
			return UpdateFromDtoByQueryAsync(e => e.JobId == jobId, dto, cancellationToken);
		}

		/// <inheritdoc />
		public Task<List<SubmissionValidationRequestDataDto>> GetRequestsForSchedulingAsync(int count,
			WorkerJobState state, IEnumerable<string> gameKeys, CancellationToken cancellationToken)
		{
			return DbSet.Where(e => e.State == state)
				.Where(e => gameKeys.Contains(e.Submission.Tournament.Game.Key))
				.OrderBy(e => e.Created)
				.Take(count)
				.ProjectTo<SubmissionValidationRequestDataDto>(Mapper.ConfigurationProvider)
				.ToListAsync(cancellationToken);
		}

		/// <inheritdoc />
		public Task<SubmissionValidationPreviewDto> FindByIdAsync(long id, CancellationToken cancellationToken)
		{
			return GetDtoByIdAsync<SubmissionValidationPreviewDto>(id, cancellationToken);
		}
	}
}