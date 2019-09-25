using OPCAIC.Application.Dtos.Users;
using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Application.Dtos.Submissions
{
	public class SubmissionScoreViewDto : IMapFrom<Submission>
	{
		public long Id { get; set; }
		public double Score { get; set; }
		public UserReferenceDto Author { get; set; }
	}
}
