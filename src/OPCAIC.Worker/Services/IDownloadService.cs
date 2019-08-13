using System.Threading;
using System.Threading.Tasks;

namespace OPCAIC.Worker.Services
{
	/// <summary>
	///     Provides methods for downloading files from file server.
	/// </summary>
	public interface IDownloadService
	{
		/// <summary>
		///     Downloads the submission archive and unpacks it to given path.
		/// </summary>
		/// <param name="submissionId">Unique id of the submission.</param>
		/// <param name="path">Path to the target directory.</param>
		/// <param name="cancellationToken">Cancellation token to be used if the task should be cancelled.</param>
		/// <returns></returns>
		Task DownloadSubmission(long submissionId, string path,
			CancellationToken cancellationToken = default);

		/// <summary>
		///     Uploads contents of a folder as a result of submission validation.
		/// </summary>
		/// <param name="validationId">Unique id of the validation.</param>
		/// <param name="path">Path to the result directory.</param>
		/// <param name="cancellationToken">Cancellation token to be used if the task should be cancelled.</param>
		/// <returns></returns>
		Task UploadValidationResults(long validationId, string path,
			CancellationToken cancellationToken = default);

		/// <summary>
		///     Uploads contents of a folder as a result of submission validation.
		/// </summary>
		/// <param name="executionId">Unique id of the match execution.</param>
		/// <param name="path">Path to the result directory.</param>
		/// <param name="cancellationToken">Cancellation token to be used if the task should be cancelled.</param>
		/// <returns></returns>
		Task UploadMatchResults(long executionId, string path,
			CancellationToken cancellationToken = default);
	}
}