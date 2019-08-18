using System.Collections.Generic;

namespace OPCAIC.ApiService.Models.Matches
{
	public class MatchExecutionModel
	{
		public long MatchId { get; set; }
		public IList<SubmissionMatchResultModel> BotResults { get; set; }
	}
}