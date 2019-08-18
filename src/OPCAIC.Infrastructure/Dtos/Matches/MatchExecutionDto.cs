using System.Collections.Generic;

namespace OPCAIC.Infrastructure.Dtos.Matches
{
	public class MatchExecutionDto
	{
		public long MatchId { get; set; }
		public IList<SubmissionMatchResultDto> BotResults { get; set; }
	}
}