using System;
using System.Collections.Generic;
using OPCAIC.Infrastructure.Dtos.Matches;
using OPCAIC.Infrastructure.Dtos.Submissions;
using OPCAIC.Infrastructure.Enums;

namespace OPCAIC.Infrastructure.Dtos.MatchExecutions
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