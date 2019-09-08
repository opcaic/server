using System;
using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Infrastructure.Dtos.SubmissionValidations;
using OPCAIC.Messaging.Messages;

namespace OPCAIC.ApiService.Services
{
	public interface ISubmissionValidationService
	{
		Task EnqueueValidationAsync(long submissionId, CancellationToken cancellationToken);
		Task UpdateFromMessage(SubmissionValidationResult result);
		Task OnValidationRequestExpired(Guid jobId);

		Task<SubmissionValidationRequest> CreateValidationRequestAsync(
			long id, CancellationToken cancellationToken);

		SubmissionValidationRequest CreateRequest(SubmissionValidationRequestDataDto data);
	}
}