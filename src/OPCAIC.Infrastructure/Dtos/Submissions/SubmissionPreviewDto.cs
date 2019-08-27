using OPCAIC.Infrastructure.Dtos.Tournaments;
using OPCAIC.Infrastructure.Dtos.Users;

namespace OPCAIC.Infrastructure.Dtos.Submissions
{
	public class SubmissionPreviewDto
	{
		public long Id { get; set; }
		public UserReferenceDto Author { get; set; }
		public TournamentReferenceDto Tournament { get; set; }
	}

	public class SubmissionAuthDto
	{
		public long AuthorId { get; set; }
		public long TournamentOwnerId { get; set; }
		public long[] TournamentManagersIds { get; set; }
	}
}