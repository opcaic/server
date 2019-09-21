using System;
using System.Threading;
using System.Threading.Tasks;
using OPCAIC.ApiService.Models.Matches;
using OPCAIC.Application.Dtos.MatchExecutions;
using OPCAIC.Messaging.Messages;

namespace OPCAIC.ApiService.Interfaces
{
	public interface IMatchExecutionService
	{
		Task UpdateFromMessage(MatchExecutionResult result);
		Task OnExecutionRequestExpired(Guid jobId);
		Task<MatchExecutionDetailModel> GetByIdAsync(long id, CancellationToken cancellationToken);
		MatchExecutionRequest CreateRequest(MatchExecutionRequestDataDto data);
	}
}