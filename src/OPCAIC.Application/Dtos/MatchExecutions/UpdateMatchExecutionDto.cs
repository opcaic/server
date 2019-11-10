using System;
using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Application.MatchExecutions.Events;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.Dtos.MatchExecutions
{
	public class UpdateMatchExecutionDto
	{
		public string AdditionalData { get; set; }
		public NewSubmissionMatchResultDto[] BotResults { get; set; }
		public EntryPointResult ExecutorResult { get; set; }
		public WorkerJobState State { get; set; }
	}
}