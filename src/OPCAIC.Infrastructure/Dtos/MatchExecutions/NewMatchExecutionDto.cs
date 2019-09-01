using System;
using OPCAIC.Infrastructure.Dtos.Submissions;

namespace OPCAIC.Infrastructure.Dtos.MatchExecutions
{
	public class NewMatchExecutionDto
	{
		public Guid JobId { get; set; }

		public long MatchId { get; set; }
	}
}