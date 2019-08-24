using System.Collections.Generic;

namespace OPCAIC.Infrastructure.Dtos.Broker
{
	public class MatchExecutionRequestDto
	{
		public List<BotInfoDto> Bots { get; set; }

		public long ExecutionId { get; set; }
	}
}