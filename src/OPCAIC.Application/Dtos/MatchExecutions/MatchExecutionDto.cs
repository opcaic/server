using System;
using System.Collections.Generic;
using OPCAIC.Application.Dtos.Matches;
using OPCAIC.Application.Dtos.Submissions;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.Dtos.MatchExecutions
{
	public class MatchExecutionDto
	{
		public long Id { get; set; }
		public MatchReferenceDto Match { get; set; }
		public IList<SubmissionMatchResultDto> BotResults { get; set; }
		public EntryPointResult ExecutorResult { get; set; }
		public WorkerJobState State { get; set; }
		public Guid JobId { get; set; }
		public DateTime? Executed { get; set; }
		public DateTime Created { get; set; }
		public string AdditionalData { get; set; }
	}
}