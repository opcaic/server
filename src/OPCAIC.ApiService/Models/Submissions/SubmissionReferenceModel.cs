using OPCAIC.ApiService.Models.Users;

namespace OPCAIC.ApiService.Models.Submissions
{
	public class SubmissionReferenceModel
	{
		public long Id { get; set; }
		public UserReferenceModel Author { get; set; }
	}
}