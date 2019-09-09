﻿using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using OPCAIC.Infrastructure.Dtos.MatchExecutions;
using OPCAIC.Infrastructure.Dtos.SubmissionValidations;
using OPCAIC.Services;

namespace OPCAIC.ApiService.Services
{
	public class LogStorageService : ILogStorageService
	{
		private readonly IStorageService storageService;

		public LogStorageService(IStorageService storageService)
		{
			this.storageService = storageService;
		}

		/// <inheritdoc />
		public SubmissionValidationLogsDto GetSubmissionValidationLogs(SubmissionValidationStorageDto storage)
		{
			var logs = new SubmissionValidationLogsDto();

			var file = storageService.ReadSubmissionValidationResultArchive(storage);
			if (file == null)
			{
				return logs;
			}

			using (var archive = new ZipArchive(file, ZipArchiveMode.Read))
			{
				foreach (var entry in archive.Entries)
				{
					switch (entry.Name)
					{
						case "check.0.stdout":
							logs.CheckerLog = ReadAsText(entry);
							break;
						case "compile.0.stdout":
							logs.CompilerLog = ReadAsText(entry);
							break;
						case "validate.0.stdout":
							logs.ValidatorLog = ReadAsText(entry);
							break;
					}
				}
			}

			return logs;
		}

		private static readonly Regex CompilerLogRegex = new Regex(@"^compile\.(\d+)\.stdout$", RegexOptions.Compiled);

		/// <inheritdoc />
		public MatchExecutionLogsDto GetMatchExecutionLogs(MatchExecutionStorageDto storage)
		{
			var logs = new MatchExecutionLogsDto();

			var file = storageService.ReadMatchResultArchive(storage);
			if (file == null)
			{
				return logs;
			}

			using (var archive = new ZipArchive(file, ZipArchiveMode.Read))
			{
				foreach (var entry in archive.Entries)
				{
					if (entry.Name == "execute.stdout")
					{
						logs.ExecutorLog = ReadAsText(entry);
					}
					else
					{
						var match = CompilerLogRegex.Match(entry.Name);
						if (!match.Success)
						{
							continue;
						}

						var order = int.Parse(match.Groups[1].Value);

						// make sure there is enough space
						while (logs.CompilerLogs.Count <= order)
						{
							logs.CompilerLogs.Add(null);
						}

						logs.CompilerLogs[order] = ReadAsText(entry);
					}
				}
			}

			return logs;
		}

		private static string ReadAsText(ZipArchiveEntry entry)
		{
			using (var stream = new StreamReader(entry.Open()))
			{
				return stream.ReadToEnd();
			}
		}
	}
}