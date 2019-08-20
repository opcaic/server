using System.Collections.Generic;
using OPCAIC.ApiService.Models.Submissions;

namespace OPCAIC.ApiService.Models.Matches
{
	public class MatchExecutionModel
	{
		public MatchReferenceModel Match { get; set; }
		public IList<SubmissionMatchResultModel> BotResults { get; set; }
	}
}