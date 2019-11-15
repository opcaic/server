using System;
using AutoMapper;
using OPCAIC.Application.Dtos.MatchExecutions;
using OPCAIC.Application.Infrastructure.AutoMapper;

namespace OPCAIC.Application.MatchExecutions.Models
{
	public class MatchExecutionDetailDto
		: MatchExecutionDetailDtoBase<MatchExecutionDetailDto.SubmissionResultDetailDto>, ICustomMapping
	{
		[IgnoreMap]
		public string ExecutorLog { get; set; }

		public class SubmissionResultDetailDto
			: MatchExecutionPreviewDto.SubmissionResultDto,
				IMapFrom<MatchExecutionPreviewDto.SubmissionResultDto>
		{
			[IgnoreMap]
			public string CompilerLog { get; set; }

			/// <inheritdoc />
			public override void AddLogs(MatchExecutionLogsDto.SubmissionLog logs)
			{
				base.AddLogs(logs);
				CompilerLog = logs.CompilerLog;
			}
		}

		/// <inheritdoc />
		public void CreateMapping(Profile configuration)
		{
			CreateCustomMapping(configuration);
		}

		/// <inheritdoc />
		public override void AddLogs(MatchExecutionLogsDto logs)
		{
			ExecutorLog = logs.ExecutorLog;
			for (int i = 0; i < Math.Min(logs.SubmissionLogs.Count, BotResults.Count); i++)
			{
				BotResults[i].AddLogs(logs.SubmissionLogs[i]);
			}
		}
	}
}