using System;
using System.Threading;
using System.Threading.Tasks;
using OPCAIC.ApiService.Models.SubmissionValidations;
using OPCAIC.Application.Dtos.SubmissionValidations;
using OPCAIC.Messaging.Messages;

namespace OPCAIC.ApiService.Interfaces
{
	public interface ISubmissionValidationService
	{
		Task EnqueueValidationAsync(long submissionId, CancellationToken cancellationToken);
		Task OnValidationRequestExpired(Guid jobId);

		SubmissionValidationRequest CreateRequest(SubmissionValidationRequestDataDto data);

		Task<SubmissionValidationDetailModel> GetByIdAsync(long id,
			CancellationToken cancellationToken);
	}
}