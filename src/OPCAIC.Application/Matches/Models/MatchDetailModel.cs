using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using OPCAIC.Application.Dtos.Submissions;
using OPCAIC.Application.Dtos.Tournaments;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.Matches.Models
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
					case EntryPointResult.NotExecuted:
						return MatchState.Queued;

					case EntryPointResult.Success:
						return MatchState.Executed;

					case EntryPointResult.UserError:
					case EntryPointResult.ModuleError:
					case EntryPointResult.PlatformError:
						return MatchState.Failed;

					case EntryPointResult.Cancelled:
						return MatchState.Cancelled;

					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		public TournamentDto Tournament { get; set; }
		public IList<SubmissionReferenceDto> Submissions { get; set; }
		public IList<ExecutionDto> Executions { get; set; }

		public class TournamentDto : TournamentReferenceDto
		{
			public TournamentFormat Format { get; set; }

			public TournamentScope Scope { get; set; }

			public TournamentRankingStrategy RankingStrategy { get; set; }
		}

		public class ExecutionDto
		{
			public long Id { get; set; }
			public EntryPointResult ExecutorResult { get; set; }
			public IList<SubmissionResultDto> BotResults { get; set; }
			public DateTime? Executed { get; set; }
			public DateTime Created { get; set; }
			public JObject AdditionalData { get; set; }

			public class SubmissionResultDto
			{
				public SubmissionReferenceDto Submission { get; set; }
				public double Score { get; set; }
				public EntryPointResult CompilerResult { get; set; }
				public JObject AdditionalData { get; set; }
			}
		}
	}
}