using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Domain.Enums;
using OPCAIC.Infrastructure.Dtos;
using OPCAIC.Infrastructure.Dtos.Broker;
using OPCAIC.Infrastructure.Dtos.SubmissionValidations;

namespace OPCAIC.Infrastructure.Repositories
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