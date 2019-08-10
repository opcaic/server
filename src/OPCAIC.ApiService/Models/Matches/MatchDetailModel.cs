using OPCAIC.Infrastructure.Dtos;

namespace OPCAIC.ApiService.Models.Matches
{
	public class MatchDetailModel
	{
		public long Id { get; set; }
		public long Index { get; set; }
		public TournamentReferenceModel Tournament { get; set; }
		public ListDto<UserReferenceModel> Participators { get; set; }
		public ListDto<SubmissionMatchResultReferenceModel> Results { get; set; }
	}
}