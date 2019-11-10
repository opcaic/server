using System.Collections.Generic;
using OPCAIC.Application.Infrastructure;
using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Application.MatchExecutions.Models;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.Matches.Models
{
	public abstract class MatchDetailDtoBase<TExecution, TSubmission> : MatchDtoBase, IMapFrom<Match>
		where TExecution : MatchExecutionDtoBase<TSubmission>
		where TSubmission : IAnonymizable
	{
		public List<TExecution> Executions { get; set; }

		/// <inheritdoc />
		public override MatchState State => Executions[^1].ComputeMatchState();

		public override void AnonymizeUsersExcept(long? userId)
		{
			base.AnonymizeUsersExcept(userId);

			foreach (var execution in Executions)
			{
				execution.AnonymizeUsersExcept(userId);
			}
		}
	}
}