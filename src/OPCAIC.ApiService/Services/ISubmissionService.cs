using System.IO;
using System.Threading;
using System.Threading.Tasks;
using OPCAIC.ApiService.Models;
using OPCAIC.ApiService.Models.Submissions;

namespace OPCAIC.ApiService.Services
{
	public interface ISubmissionService
	{
		Task<long> CreateAsync(NewSubmissionModel model, long userId,
			CancellationToken cancellationToken);

		Task<ListModel<SubmissionPreviewModel>> GetByFilterAsync(
			SubmissionFilterModel filter, CancellationToken cancellationToken);

		Task<SubmissionDetailModel> GetByIdAsync(long id,
			CancellationToken cancellationToken);

		Task<Stream> GetSubmissionArchiveAsync(long id, CancellationToken cancellationToken);

		Task UpdateAsync(long id, UpdateSubmissionModel model,
			CancellationToken cancellationToken);
	}
}