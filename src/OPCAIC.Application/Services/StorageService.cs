using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Options;
using OPCAIC.Application.Dtos.Base;
using OPCAIC.Application.Dtos.BaseDtos;
using OPCAIC.Application.Interfaces;
using OPCAIC.Utils;

namespace OPCAIC.Application.Services
{
	public class StorageService : IStorageService
	{

		private readonly DirectoryInfo directory;

		public StorageService(IOptions<StorageConfiguration> config)
		{
			directory = Directory.CreateDirectory(config.Value.Directory);
		}

		/// <inheritdoc />
		public Stream ReadSubmissionArchive(SubmissionDtoBase submission)
		{
			return ReadFile(SubmissionPath(submission));
		}

		/// <inheritdoc />
		public Stream WriteSubmissionArchive(SubmissionDtoBase submission)
		{
			return WriteFile(SubmissionPath(submission));
		}

		/// <inheritdoc />
		public void DeleteSubmissionArchive(SubmissionDtoBase submission)
		{
			DeleteFile(SubmissionPath(submission));
		}

		/// <inheritdoc />
		public Stream ReadMatchResultArchive(MatchExecutionDtoBase matchExecution)
		{
			return ReadFile(MatchResultPath(matchExecution));
		}

		/// <inheritdoc />
		public Stream WriteMatchResultArchive(MatchExecutionDtoBase matchExecution)
		{
			return WriteFile(MatchResultPath(matchExecution));
		}

		/// <inheritdoc />
		public void DeleteMatchResultArchive(MatchExecutionDtoBase matchExecution)
		{
			DeleteFile(MatchResultPath(matchExecution));
		}

		/// <inheritdoc />
		public Stream WriteSubmissionValidationResultArchive(SubmissionValidationDtoBase validation)
		{
			return WriteFile(ValidationPath(validation));
		}

		/// <inheritdoc />
		public Stream ReadSubmissionValidationResultArchive(SubmissionValidationDtoBase validation)
		{
			return ReadFile(ValidationPath(validation));
		}

		/// <inheritdoc />
		public void DeleteSubmissionValidationResultArchive(SubmissionValidationDtoBase validation)
		{
			DeleteFile(ValidationPath(validation));
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

		/// <inheritdoc />
		public void DeleteTournamentAdditionalFiles(long id)
		{
			DeleteFile(TournamentFilesPath(id));
		}

		/// <inheritdoc />
		public void DeleteAllTournamentFiles(long id)
		{
			var path = TournamentPath(id);
			if (Directory.Exists(path))
			{
				Directory.Delete(path, true);
			}
		}

		private string TournamentPath(long id)
		{
			return Path.Combine(directory.FullName, $"tournament-{id}");
		}

		private string TournamentFilesPath(long id)
		{
			return Path.Combine(TournamentPath(id), "additionalFiles.zip");
		}

		private string SubmissionPath(SubmissionDtoBase sub)
		{
			return Path.Combine(TournamentPath(sub.TournamentId), "submissions", $"submission-{sub.Id}", "submission.zip");
		}

		private string ValidationPath(SubmissionValidationDtoBase dto)
		{
			return Path.Combine(TournamentPath(dto.TournamentId), "submissions", $"submission-{dto.SubmissionId}", $"validation-{dto.Id}.zip");
		}

		private string MatchResultPath(MatchExecutionDtoBase m)
		{
			return Path.Combine(TournamentPath(m.TournamentId), "matches", $"match-{m.MatchId}", $"execution-{m.Id}.zip");
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

			// make sure the dir exists
			Directory.CreateDirectory(Path.GetDirectoryName(path));
			return File.Open(path, overwrite ? FileMode.Create : FileMode.CreateNew, FileAccess.Write);
		}

		private void DeleteFile(string path)
		{
			Require.ArgNotNull(path, nameof(path));
			if (File.Exists(path))
			{
				File.Delete(path);
			}
		}
	}
}