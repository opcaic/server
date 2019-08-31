using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Messaging.Messages;

namespace OPCAIC.ApiService.Services
{
	public interface ISubmissionValidationService
	{
		Task EnqueueValidationAsync(long submissionId, CancellationToken cancellationToken);
		Task UpdateFromMessage(SubmissionValidationResult result);
	}
}