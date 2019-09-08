using System;
using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Infrastructure.Dtos.MatchExecutions;
using OPCAIC.Infrastructure.Dtos.SubmissionValidations;
using OPCAIC.Messaging.Messages;

namespace OPCAIC.ApiService.Services
{
	public interface IMatchExecutionService
	{
		Task EnqueueExecutionAsync(long matchId, CancellationToken cancellationToken);
		Task UpdateFromMessage(MatchExecutionResult result);
		Task OnExecutionRequestExpired(Guid jobId);

		Task<MatchExecutionRequest> CreateValidationRequestAsync(
			long id, CancellationToken cancellationToken);

		MatchExecutionRequest CreateRequest(MatchExecutionRequestDataDto data);
	}
}