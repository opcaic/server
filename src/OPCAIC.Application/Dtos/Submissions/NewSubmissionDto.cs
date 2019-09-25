using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Application.Dtos.Submissions
{
	public class NewSubmissionDto : IMapTo<Submission>
	{
		public long AuthorId { get; set; }
		public long TournamentId { get; set; }
		public long Score { get; set; }
	}
}