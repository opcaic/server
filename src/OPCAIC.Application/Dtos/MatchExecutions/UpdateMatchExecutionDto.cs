using System;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.Dtos.MatchExecutions
{
	public class UpdateMatchExecutionDto
	{
		public string AdditionalData { get; set; }
		public NewSubmissionMatchResultDto[] BotResults { get; set; }
		public EntryPointResult ExecutorResult { get; set; }
		public WorkerJobState State { get; set; }
		public DateTime? Executed { get; set; }
	}
}