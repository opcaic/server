using System.Collections.Generic;

namespace OPCAIC.Application.Dtos.MatchExecutions
{
	public class MatchExecutionRequestDataDto : JobDtoBase
	{
		public List<long> SubmissionIds { get; set; }
		public long MatchId { get; set; }
	}
}