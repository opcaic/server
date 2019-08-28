using System;
using System.Collections.Generic;
using OPCAIC.Infrastructure.Dtos.Submissions;

namespace OPCAIC.Infrastructure.Dtos.Matches
{
	public class MatchExecutionDto
	{
		public MatchReferenceDto Match { get; set; }
		public IList<SubmissionMatchResultDto> BotResults { get; set; }
		public DateTime? Executed { get; set; }
	}
}