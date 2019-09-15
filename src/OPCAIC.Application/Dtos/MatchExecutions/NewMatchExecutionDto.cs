using System;

namespace OPCAIC.Application.Dtos.MatchExecutions
{
	public class NewMatchExecutionDto
	{
		public Guid JobId { get; set; }

		public long MatchId { get; set; }
	}
}