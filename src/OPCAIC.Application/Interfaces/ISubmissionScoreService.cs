using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Application.MatchExecutions.Events;

namespace OPCAIC.Application.Interfaces
{
	public interface ISubmissionScoreService
	{
		Task UpdateSubmissionsScore(MatchExecutionFinished result, CancellationToken cancellationToken);
	}
}