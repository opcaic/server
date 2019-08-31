using System;
using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Infrastructure.Dtos.Broker;
using OPCAIC.Infrastructure.Dtos.SubmissionValidations;

namespace OPCAIC.Infrastructure.Repositories
{
	public interface ISubmissionValidationRepository
		: ICreateRepository<NewSubmissionValidationDto>
	{
		Task<SubmissionValidationStorageDto> FindStorageAsync(long id, CancellationToken cancellationToken);

		Task<bool> UpdateFromJobAsync(Guid jobId, UpdateSubmissionValidationDto dto,
			CancellationToken cancellationToken);
	}
}