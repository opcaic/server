using OPCAIC.Infrastructure.Dtos.Users;

namespace OPCAIC.Infrastructure.Dtos.Submissions
{
	public class SubmissionReferenceDto
	{
		public long Id { get; set; }
		public UserReferenceDto Author { get; set; }
	}
}