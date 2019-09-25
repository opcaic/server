using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Application.Dtos.Submissions
{
	public class UpdateSubmissionScoreDto : IMapTo<Submission>
	{
		public double Score { get; set; }
	}
}