using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Application.Dtos;
using OPCAIC.Application.Dtos.SubmissionValidations;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.Interfaces.Repositories
{
	public interface ISubmissionValidationRepository
		: ICreateRepository<NewSubmissionValidationDto>,
			IAuthDataRepository<SubmissionValidationAuthDto>,
			ILookupRepository<SubmissionValidationDto>
	{
		Task<SubmissionValidationStorageDto> FindStorageAsync(long id,
			CancellationToken cancellationToken);

		Task<bool> UpdateFromJobAsync(Guid jobId, UpdateSubmissionValidationDto dto,
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