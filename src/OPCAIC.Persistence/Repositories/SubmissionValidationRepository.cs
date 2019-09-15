﻿using System;
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
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Persistence.Repositories
{
	public class SubmissionValidationRepository
		: RepositoryBase<SubmissionValidation>, ISubmissionValidationRepository
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

		public Task<bool> UpdateFromJobAsync(Guid jobId, UpdateSubmissionValidationDto dto,
			CancellationToken cancellationToken)
		{
			return UpdateFromDtoByQueryAsync(v => v.JobId == jobId, dto, cancellationToken);
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

		public Task<SubmissionValidationRequestDataDto> GetRequestDataAsync(long id,
			CancellationToken cancellationToken)
		{
			return GetDtoByIdAsync<SubmissionValidationRequestDataDto>(id, cancellationToken);
		}

		/// <inheritdoc />
		public Task<SubmissionValidationDto> FindByIdAsync(long id, CancellationToken cancellationToken)
		{
			return GetDtoByIdAsync<SubmissionValidationDto>(id, cancellationToken);
		}

		/// <inheritdoc />
		public Task<SubmissionValidationAuthDto> GetAuthorizationData(long id, CancellationToken cancellationToken = default)
		{
			return GetDtoByIdAsync<SubmissionValidationAuthDto>(id, cancellationToken);
		}
	}
}