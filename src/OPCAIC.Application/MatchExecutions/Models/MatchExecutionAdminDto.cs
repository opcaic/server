using System;
using System.Collections.Generic;
using AutoMapper;
using OPCAIC.Application.Dtos.MatchExecutions;
using OPCAIC.Application.Infrastructure;

namespace OPCAIC.Application.MatchExecutions.Models
{
	public class MatchExecutionAdminDto
		: MatchExecutionDetailDtoBase<MatchExecutionAdminDto.SubmissionResultAdminDto>
	{
		[IgnoreMap]
		public string ExecutorLog { get; set; }

		[IgnoreMap]
		public string ExecutorErrorLog { get; set; }

		public string Exception { get; set; }

		public override void AddLogs(MatchExecutionLogsDto logs)
		{
			ExecutorLog = logs.ExecutorLog;
			ExecutorErrorLog = logs.ExecutorErrorLog;
			for (int i = 0; i < Math.Min(logs.SubmissionLogs.Count, BotResults.Count); i++)
			{
				BotResults[i].AddLogs(logs.SubmissionLogs[i]);
			}
		}

		public class SubmissionResultAdminDto : MatchExecutionDetailDto.SubmissionResultDetailDto
		{
			[IgnoreMap]
			public string CompilerErrorLog { get; set; }

			/// <inheritdoc />
			public override void AddLogs(MatchExecutionLogsDto.SubmissionLog logs)
			{
				base.AddLogs(logs);
				CompilerErrorLog = logs.CompilerErrorLog;
			}
		}
	}
}