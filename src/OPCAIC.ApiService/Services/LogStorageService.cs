using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using OPCAIC.Application.Dtos.MatchExecutions;
using OPCAIC.Application.Dtos.SubmissionValidations;
using OPCAIC.Application.Interfaces;

namespace OPCAIC.ApiService.Services
{
	public class LogStorageService : ILogStorageService
	{
		private static readonly Regex compilerLogRegex =
			new Regex(@"^compile\.(\d+)\.(stdout|stderr)$", RegexOptions.Compiled);

		private readonly IStorageService storageService;

		public LogStorageService(IStorageService storageService)
		{
			this.storageService = storageService;
		}

		/// <inheritdoc />
		public SubmissionValidationLogsDto GetSubmissionValidationLogs(
			SubmissionValidationStorageDto storage)
		{
			var logs = new SubmissionValidationLogsDto();

			var file = storageService.ReadSubmissionValidationResultArchive(storage);
			if (file == null)
			{
				return logs;
			}

			using var archive = new ZipArchive(file, ZipArchiveMode.Read);

			foreach (var entry in archive.Entries)
			{
				switch (entry.Name)
				{
					case "check.0.stdout":
						logs.CheckerLog = ReadAsText(entry);
						break;
					case "check.0.stderr":
						logs.CheckerErrorLog = ReadAsText(entry);
						break;
					case "compile.0.stdout":
						logs.CompilerLog = ReadAsText(entry);
						break;
					case "compile.0.stderr":
						logs.CompilerErrorLog = ReadAsText(entry);
						break;
					case "validate.0.stdout":
						logs.ValidatorLog = ReadAsText(entry);
						break;
					case "validate.0.stderr":
						logs.ValidatorErrorLog = ReadAsText(entry);
						break;
				}
			}

			return logs;
		}

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
					switch (entry.Name)
					{
						case "execute.stdout":
							logs.ExecutorLog = ReadAsText(entry);
							break;
						case "execute.stderr":
							logs.ExecutorErrorLog = ReadAsText(entry);
							break;
						default:
							var match = compilerLogRegex.Match(entry.Name);
							if (!match.Success)
							{
								continue;
							}

							var order = int.Parse(match.Groups[1].Value);

							// make sure there is enough space
							while (logs.SubmissionLogs.Count <= order)
							{
								logs.SubmissionLogs.Add(new MatchExecutionLogsDto.SubmissionLog());
							}

							if (match.Groups[2].Value == "stdout")
							{
								logs.SubmissionLogs[order].CompilerLog = ReadAsText(entry);
							}
							else
							{
								logs.SubmissionLogs[order].CompilerErrorLog = ReadAsText(entry);
							}

							break;
					}
				}
			}

			return logs;
		}

		private static string ReadAsText(ZipArchiveEntry entry)
		{
			using var stream = new StreamReader(entry.Open());
			return stream.ReadToEnd();
		}
	}
}