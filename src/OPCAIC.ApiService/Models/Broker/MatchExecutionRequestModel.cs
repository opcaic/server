using System.Collections.Generic;

namespace OPCAIC.ApiService.Models.Broker
{
	public class MatchExecutionRequestModel : WorkMessageBaseModel
	{
		public List<BotInfoModel> Bots { get; set; }

		public long ExecutionId { get; set; }
	}
}