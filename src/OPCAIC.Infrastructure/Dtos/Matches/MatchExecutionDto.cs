using System;
using System.Collections.Generic;
using OPCAIC.Infrastructure.Dtos.Submissions;
using OPCAIC.Infrastructure.Enums;

namespace OPCAIC.Infrastructure.Dtos.Matches
{
	public class MatchExecutionDto
	{
		public MatchReferenceDto Match { get; set; }
		public IList<SubmissionMatchResultDto> BotResults { get; set; }
		public GameModuleEntryPointResult ExecutorResult { get; set; }
		public DateTime? Executed { get; set; }
	}
}