using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using OPCAIC.Application.Dtos.Submissions;
using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Application.Matches.Models;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.Dtos.Tournaments
{
	public class TournamentGenerationDtoBase : ICustomMapping
	{
		public long Id { get; set; }
		public TournamentFormat Format { get; set; }
		public List<long> ActiveSubmissionIds { get; set; }

		/// <inheritdoc />
		void ICustomMapping.CreateMapping(Profile configuration)
		{
			configuration.CreateMap<Tournament, TournamentGenerationDtoBase>(MemberList.Destination)
				.ForMember(d => d.ActiveSubmissionIds,
					opt => opt.MapFrom(f
						=> f.Participants.Where(s => s.ActiveSubmissionId != null)
							.Select(s => s.ActiveSubmissionId.Value)))
				.IncludeAllDerived();
		}
	}

	public class TournamentBracketsGenerationDto : TournamentGenerationDtoBase, IMapFrom<Tournament>
	{
		public List<MatchDetailDto> Matches { get; set; }
	}

	public class TournamentDeadlineGenerationDto : TournamentGenerationDtoBase, IMapFrom<Tournament>
	{
	}

	public class TournamentOngoingGenerationDto : TournamentGenerationDtoBase, ICustomMapping
	{
		public int MatchesCount { get; set; }
		public DateTime EvaluationStarted { get; set; }
		public int MatchesPerDay { get; set; }
		public List<SubmissionScoreViewDto> Submissions { get; set; }

		/// <inheritdoc />
		void ICustomMapping.CreateMapping(Profile configuration)
		{
			configuration.CreateMap<Tournament, TournamentOngoingGenerationDto>(MemberList.Destination)
				.IncludeBase<Tournament, TournamentGenerationDtoBase>()
				.ForMember(t => t.Submissions,
					opt => opt.MapFrom(x => x.Participants.Where(p => p.ActiveSubmission != null).Select(p => p.ActiveSubmission)));

		}
	}
}