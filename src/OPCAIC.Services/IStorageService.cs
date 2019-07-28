using System;
using System.IO;
using OPCAIC.Infrastructure.Dtos;
using OPCAIC.Infrastructure.Entities;

namespace OPCAIC.Services
{
	/// <summary>
	///     Provides methods for reading various user files stored on the server.
	/// </summary>
	public interface IStorageService
	{
		/// <summary>
		///     Opens stream for archive containing files for given submission. Returns null when no such archive exists.
		/// </summary>
		/// <param name="submission"></param>
		/// <returns></returns>
		Stream ReadSubmissionArchive(SubmissionStorageDto submission);

		/// <summary>
		///     Opens stream for writing archive for given submission.
		/// </summary>
		/// <param name="submission"></param>
		/// <exception cref="InvalidOperationException">When such file already exists.</exception>
		/// <returns></returns>
		Stream WriteSubmissionArchive(SubmissionStorageDto submission);

		/// <summary>
		///     Opens stream for archive containing result files from given match execution. Returns null when no such archive exists.
		/// </summary>
		/// <param name="matchExecution"></param>
		/// <returns></returns>
		Stream ReadMatchResultArchive(MatchExecutionStorageDto matchExecution);

		/// <summary>
		///     Opens stream for writing archive for given match result.
		/// </summary>
		/// <param name="matchExecution"></param>
		/// <exception cref="InvalidOperationException">When such file already exists.</exception>
		/// <returns></returns>
		Stream WriteMatchResultArchive(MatchExecutionStorageDto matchExecution);
	}
}