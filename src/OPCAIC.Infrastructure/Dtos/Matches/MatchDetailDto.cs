using System;
using System.Collections.Generic;
using System.Linq;
using OPCAIC.Infrastructure.Dtos.Submissions;
using OPCAIC.Infrastructure.Dtos.Tournaments;
using OPCAIC.Infrastructure.Enums;

namespace OPCAIC.Infrastructure.Dtos.Matches
{
	public class MatchDetailDto
	{
		public long Id { get; set; }
		public long Index { get; set; }

		public MatchState State
		{
			get
			{
				switch (Executions.LastOrDefault()?.ExecutorResult)
				{
					case null:
					case GameModuleEntryPointResult.NotExecuted:
						return MatchState.Queued;

					case GameModuleEntryPointResult.Success:
						return MatchState.Executed;

					case GameModuleEntryPointResult.UserError:
					case GameModuleEntryPointResult.ModuleError:
					case GameModuleEntryPointResult.PlatformError:
						return MatchState.Failed;

					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		public TournamentReferenceDto Tournament { get; set; }
		public IList<SubmissionReferenceDto> Submissions { get; set; }
		public IList<MatchExecutionDto> Executions { get; set; }
	}
}