﻿using System;
using System.IO;
using OPCAIC.Application.Dtos.MatchExecutions;
using OPCAIC.Application.Dtos.Submissions;
using OPCAIC.Application.Dtos.SubmissionValidations;

namespace OPCAIC.Application.Interfaces
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
		///     Opens stream for archive containing result files from given match execution. Returns null when no such archive
		///     exists.
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

		/// <summary>
		///     Opens stream for writing archive for given submission validation.
		/// </summary>
		/// <param name="validation"></param>
		/// <returns></returns>
		Stream WriteSubmissionValidationResultArchive(SubmissionValidationStorageDto validation);

		/// <summary>
		///     Opens stream for reading archive for given submission validation.
		/// </summary>
		/// <param name="validation"></param>
		/// <returns></returns>
		Stream ReadSubmissionValidationResultArchive(SubmissionValidationStorageDto validation);

		/// <summary>
		///     Opens stream for reading archive for additional files for given tournament.
		/// </summary>
		/// <param name="id">Id of the tournament.</param>
		/// <returns></returns>
		Stream ReadTournamentAdditionalFiles(long id);

		/// <summary>
		///     Opens stream for writing archive for additional files for given tournament.
		/// </summary>
		/// <param name="id">Id of the tournament.</param>
		/// <param name="overwrite">Whether an existing file should be overwritten.</param>
		/// <returns></returns>
		Stream WriteTournamentAdditionalFiles(long id, bool overwrite = false);
	}
}