using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Messaging.Messages;

namespace OPCAIC.Application.Interfaces
{
	public interface ISubmissionScoreService
	{
		Task UpdateSubmissionsScore(MatchExecutionResult result, CancellationToken cancellationToken);
	}
}