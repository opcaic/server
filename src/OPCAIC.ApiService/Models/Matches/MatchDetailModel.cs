using System.Collections.Generic;
using OPCAIC.ApiService.Models.Submissions;
using OPCAIC.ApiService.Models.Tournaments;
using OPCAIC.Infrastructure.Enums;

namespace OPCAIC.ApiService.Models.Matches
{
	public class MatchDetailModel
	{
		public long Id { get; set; }
		public long Index { get; set; }
		public MatchState State { get; set; }
		public TournamentReferenceModel Tournament { get; set; }
		public IList<SubmissionReferenceModel> Submissions { get; set; }
		public IList<MatchExecutionModel> Executions { get; set; }
	}
}