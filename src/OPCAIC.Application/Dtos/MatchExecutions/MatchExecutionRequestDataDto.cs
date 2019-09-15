using System.Collections.Generic;

namespace OPCAIC.Application.Dtos.MatchExecutions
{
	public class MatchExecutionRequestDataDto : JobDtoBase
	{
		public long Id { get; set; }
		public List<long> SubmissionIds { get; set; }
	}
}