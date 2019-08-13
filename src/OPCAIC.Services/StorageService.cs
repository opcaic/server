﻿using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Options;
using OPCAIC.Infrastructure.Dtos;
using OPCAIC.Utils;

namespace OPCAIC.Services
{
	public class StorageService : IStorageService
	{
		private const string SubmissionsDir = "Submissions";
		private const string ResultsDir = "Results";
		private readonly DirectoryInfo directory;

		public StorageService(IOptions<StorageConfiguration> config)
		{
			directory = Directory.CreateDirectory(config.Value.Directory);
			directory.CreateSubdirectory(SubmissionsDir);
			directory.CreateSubdirectory(ResultsDir);
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

		// TODO: choose better folder hierarchy once we have complete domain model
		private string SubmissionPath(SubmissionStorageDto sub)
		{
			return Path.Combine(directory.FullName, SubmissionsDir, $"{sub.Id}.zip");
		}

		private string MatchResultPath(MatchExecutionStorageDto m)
		{
			return Path.Combine(directory.FullName, ResultsDir, $"{m.Id}.zip");
		}

		private Stream ReadFile(string path)
		{
			Require.ArgNotNull(path, nameof(path));
			Debug.Assert(Path.IsPathFullyQualified(path));

			return File.Exists(path)
				? File.Open(path, FileMode.Open, FileAccess.Read)
				: null;
		}

		private Stream WriteFile(string path)
		{
			Require.ArgNotNull(path, nameof(path));
			Require.That<InvalidOperationException>(!File.Exists(path),
				$"File {path} already exists");
			Debug.Assert(Path.IsPathFullyQualified(path));

			return File.Open(path, FileMode.CreateNew, FileAccess.Write);
		}
	}
}