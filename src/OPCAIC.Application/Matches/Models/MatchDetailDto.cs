using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AutoMapper;
using Newtonsoft.Json.Linq;
using OPCAIC.Application.Dtos.Submissions;
using OPCAIC.Application.Dtos.Tournaments;
using OPCAIC.Application.Dtos.Users;
using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Application.Tournaments.Models;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.Matches.Models
{
	[DebuggerDisplay("Id = {Id}, Index = {Index}, State = {State}")]
	public class MatchDetailDto : ICustomMapping
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

		/// <inheritdoc />
		void ICustomMapping.CreateMapping(Profile configuration)
		{
			configuration.CreateMap<Match, MatchDetailDto>(MemberList.Destination)
				.ForMember(d => d.Submissions,
					opt => opt.MapFrom(m => m.Participations.Select(p => p.Submission)))
				.IncludeAllDerived();
		}

		public class TournamentDto : TournamentReferenceDto, IMapFrom<Tournament>
		{
			public TournamentFormat Format { get; set; }

			public TournamentScope Scope { get; set; }

			public TournamentRankingStrategy RankingStrategy { get; set; }
		}

		public class ExecutionDto : IMapFrom<MatchExecution>
		{
			public long Id { get; set; }
			public EntryPointResult ExecutorResult { get; set; }
			public IList<SubmissionResultDto> BotResults { get; set; }
			public DateTime? Executed { get; set; }
			public DateTime Created { get; set; }
			public JObject AdditionalData { get; set; }

			public class SubmissionResultDto : IMapFrom<SubmissionMatchResult>
			{
				public SubmissionReferenceDto Submission { get; set; }
				public double Score { get; set; }
				public EntryPointResult CompilerResult { get; set; }
				public JObject AdditionalData { get; set; }
			}
		}

		public void AnonymizeUsersExcept(long? userId)
		{
			foreach (var sub in Executions
				.SelectMany(e => e.BotResults.Select(s => s.Submission))
				.Concat(Submissions))
			{
				if (sub.Author.Id != userId)
				{
					sub.Author = UserReferenceDto.Anonymous;
				}
			}
		}
	}
}