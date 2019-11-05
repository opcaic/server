using System;
using System.Threading.Tasks;
using OPCAIC.Application.Dtos.MatchExecutions;
using OPCAIC.Messaging.Messages;

namespace OPCAIC.ApiService.Interfaces
{
	public interface IMatchExecutionService
	{
		Task UpdateFromMessage(MatchExecutionResult result);
		Task OnExecutionRequestExpired(Guid jobId);
		MatchExecutionRequest CreateRequest(MatchExecutionRequestDataDto data);
	}
}