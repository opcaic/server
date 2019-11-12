using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Application.MatchExecutions.Models;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.Matches.Models
{
	public class MatchDetailDto : MatchDtoBase, IMapFrom<Match>
	{
		public override MatchState State => LastExecution?.ComputeMatchState() ?? MatchState.Failed;

		public MatchExecutionDetailDto LastExecution { get; set; }

		/// <inheritdoc />
		public override void AnonymizeUsersExcept(long? userId)
		{
			base.AnonymizeUsersExcept(userId);

			LastExecution.AnonymizeUsersExcept(userId);
		}
	}
}