using System.Collections.Generic;

namespace OPCAIC.ApiService.Models.Matches
{
	public class MatchDetailModel
	{
		public long Id { get; set; }
		public long Index { get; set; }
		public TournamentReferenceModel Tournament { get; set; }
		public IList<UserParticipationModel> Participators { get; set; }
		public IList<SubmissionMatchResultReferenceModel> Results { get; set; }
	}
}