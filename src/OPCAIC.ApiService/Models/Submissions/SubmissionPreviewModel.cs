using OPCAIC.ApiService.Models.Tournaments;
using OPCAIC.ApiService.Models.Users;

namespace OPCAIC.ApiService.Models.Submissions
{
	public class SubmissionPreviewModel
	{
		public long Id { get; set; }
		public UserReferenceModel Author { get; set; }
		public TournamentReferenceModel Tournament { get; set; }
	}
}