using OPCAIC.Application.Dtos.Users;

namespace OPCAIC.Application.Dtos.Submissions
{
	public class SubmissionReferenceDto
	{
		public long Id { get; set; }
		public UserReferenceDto Author { get; set; }
	}
}