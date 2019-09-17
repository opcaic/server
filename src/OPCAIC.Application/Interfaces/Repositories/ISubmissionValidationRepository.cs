using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Application.Dtos;
using OPCAIC.Application.Dtos.SubmissionValidations;
using OPCAIC.Application.SubmissionValidations.Events;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.Interfaces.Repositories
{
	public interface ISubmissionValidationRepository
		: ICreateRepository<NewSubmissionValidationDto>,
			IAuthDataRepository<SubmissionValidationAuthDto>,
			ILookupRepository<SubmissionValidationDto>,
			IAsyncRepository<SubmissionValidation>
	{
		Task<SubmissionValidationStorageDto> FindStorageAsync(long id,
			CancellationToken cancellationToken);

		Task<SubmissionValidationDto> FindByGuidAsync(Guid guid,
			CancellationToken cancellationToken);

		Task<bool> UpdateFromJobAsync(Guid jobId, SubmissionValidationFinished dto,
			CancellationToken cancellationToken);

		Task<bool> UpdateJobStateAsync(Guid jobId, JobStateUpdateDto dto,
			CancellationToken cancellationToken);

		Task<List<SubmissionValidationRequestDataDto>> GetRequestsForSchedulingAsync(int count,
			WorkerJobState state,
			IEnumerable<string> gameKeys,
			CancellationToken cancellationToken);

		Task<SubmissionValidationRequestDataDto> GetRequestDataAsync(long id,
			CancellationToken cancellationToken);
	}
}