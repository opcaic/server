﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Application.Dtos;
using OPCAIC.Application.Dtos.BaseDtos;
using OPCAIC.Application.Dtos.SubmissionValidations;
using OPCAIC.Application.Specifications;
using OPCAIC.Application.SubmissionValidations.Models;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.Interfaces.Repositories
{
	public interface ISubmissionValidationRepository
		: ICreateRepository<NewSubmissionValidationDto>,
			ILookupRepository<SubmissionValidationPreviewDto>,
			IRepository<SubmissionValidation>
	{
		Task<SubmissionValidationDtoBase> FindStorageAsync(long id,
			CancellationToken cancellationToken);

		Task<bool> UpdateJobStateAsync(Guid jobId, JobStateUpdateDto dto,
			CancellationToken cancellationToken);

		Task<List<SubmissionValidationRequestDataDto>> GetRequestsForSchedulingAsync(int count,
			WorkerJobState state,
			IEnumerable<string> gameKeys,
			CancellationToken cancellationToken);
	}
}