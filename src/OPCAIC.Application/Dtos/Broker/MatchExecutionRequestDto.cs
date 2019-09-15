using System.Collections.Generic;

namespace OPCAIC.Application.Dtos.Broker
{
	public class MatchExecutionRequestDto : WorkMessageBaseDto
	{
		public List<BotInfoDto> Bots { get; set; }

		public long ExecutionId { get; set; }
	}
}