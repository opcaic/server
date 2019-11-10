using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AutoMapper;
using OPCAIC.Application.Dtos.Submissions;
using OPCAIC.Application.Dtos.Users;
using OPCAIC.Application.Infrastructure;
using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Application.MatchExecutions.Models;
using OPCAIC.Application.Tournaments.Models;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.Matches.Models
{
	[DebuggerDisplay("Id = {Id}, Index = {Index}, State = {State}")]

	public abstract class MatchDtoBase : ICustomMapping, IAnonymizable
	{
		public long Id { get; set; }
		public long Index { get; set; }

		public abstract MatchState State { get; }

		public TournamentDto Tournament { get; set; }
		public IList<SubmissionReferenceDto> Submissions { get; set; }

		public virtual void AnonymizeUsersExcept(long? userId)
		{
			foreach (var sub in Submissions)
			{
				if (sub.Author.Id != userId)
				{
					sub.Author = UserReferenceDto.Anonymous;
				}
			}
		}

		/// <inheritdoc />
		void ICustomMapping.CreateMapping(Profile configuration)
		{
			configuration.CreateMap<Match, MatchDtoBase>(MemberList.Destination)
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
	}

	public class MatchPreviewDto : MatchDtoBase, IMapFrom<Match>
	{
		public override MatchState State => LastExecution.ComputeMatchState();

		public MatchExecutionPreviewDto LastExecution { get; set; }

	}
}