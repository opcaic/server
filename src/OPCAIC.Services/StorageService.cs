using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Options;
using OPCAIC.Infrastructure.Dtos;
using OPCAIC.Infrastructure.Dtos.SubmissionValidations;
using OPCAIC.Utils;

namespace OPCAIC.Services
{
	public class StorageService : IStorageService
	{
		private const string SubmissionsDir = "Submissions";
		private const string MatchResultsDir = "Results";
		private const string ValidationsDir = "Validations";
		private const string TournamentFilesDir = "Tournaments";

		private readonly DirectoryInfo directory;

		public StorageService(IOptions<StorageConfiguration> config)
		{
			directory = Directory.CreateDirectory(config.Value.Directory);
			directory.CreateSubdirectory(SubmissionsDir);
			directory.CreateSubdirectory(MatchResultsDir);
			directory.CreateSubdirectory(ValidationsDir);
			directory.CreateSubdirectory(TournamentFilesDir);
		}

		/// <inheritdoc />
		public Stream ReadSubmissionArchive(SubmissionStorageDto submission)
		{
			return ReadFile(SubmissionPath(submission));
		}

		/// <inheritdoc />
		public Stream WriteSubmissionArchive(SubmissionStorageDto submission)
		{
			return WriteFile(SubmissionPath(submission));
		}

		/// <inheritdoc />
		public Stream ReadMatchResultArchive(MatchExecutionStorageDto matchExecution)
		{
			return ReadFile(MatchResultPath(matchExecution));
		}

		/// <inheritdoc />
		public Stream WriteMatchResultArchive(MatchExecutionStorageDto matchExecution)
		{
			return WriteFile(MatchResultPath(matchExecution));
		}

		/// <inheritdoc />
		public Stream WriteSubmissionValidationResultArchive(SubmissionValidationStorageDto validation)
		{
			return WriteFile(ValidationPath(validation));
		}

		/// <inheritdoc />
		public Stream ReadSubmissionValidationResultArchive(SubmissionValidationStorageDto validation)
		{
			return ReadFile(ValidationPath(validation));
		}

		/// <inheritdoc />
		public Stream ReadTournamentAdditionalFiles(long id)
		{
			return ReadFile(TournamentFilesPath(id));
		}

		/// <inheritdoc />
		public Stream WriteTournamentAdditionalFiles(long id, bool overwrite)
		{
			return WriteFile(TournamentFilesPath(id), overwrite);
		}

		// TODO: choose better folder hierarchy once we have complete domain model
		private string TournamentFilesPath(long id)
		{
			return Path.Combine(directory.FullName, TournamentFilesDir, $"{id}.zip");
		}

		private string SubmissionPath(SubmissionStorageDto sub)
		{
			return Path.Combine(directory.FullName, SubmissionsDir, $"{sub.Id}.zip");
		}

		private string MatchResultPath(MatchExecutionStorageDto m)
		{
			return Path.Combine(directory.FullName, MatchResultsDir, $"{m.Id}.zip");
		}

		private string ValidationPath(SubmissionValidationStorageDto dto)
		{
			return Path.Combine(directory.FullName, ValidationsDir, $"{dto.Id}.zip");
		}

		private Stream ReadFile(string path)
		{
			Require.ArgNotNull(path, nameof(path));
			Debug.Assert(Path.IsPathFullyQualified(path));

			return File.Exists(path)
				? File.Open(path, FileMode.Open, FileAccess.Read)
				: null;
		}

		private Stream WriteFile(string path, bool overwrite = false)
		{
			Require.ArgNotNull(path, nameof(path));
			Require.That<InvalidOperationException>(overwrite || !File.Exists(path),
				$"File {path} already exists");
			Debug.Assert(Path.IsPathFullyQualified(path));

			return File.Open(path, overwrite ? FileMode.Create : FileMode.CreateNew, FileAccess.Write);
		}
	}
}