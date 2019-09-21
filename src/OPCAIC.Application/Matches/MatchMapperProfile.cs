using System.Linq;
using AutoMapper;
using OPCAIC.Application.Matches.Models;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Application.Matches
{
	public class MatchMapperProfile : Profile
	{
		public MatchMapperProfile()
		{
			CreateMap<Match, MatchDetailDto>(MemberList.Destination)
				.ForMember(d => d.Submissions,
					opt => opt.MapFrom(m => m.Participations.Select(p => p.Submission)));
			CreateMap<Tournament, MatchDetailDto.TournamentDto>(MemberList.Destination);
			CreateMap<MatchExecution, MatchDetailDto.ExecutionDto>(MemberList.Destination);
			CreateMap<SubmissionMatchResult, MatchDetailDto.ExecutionDto.SubmissionResultDto>(MemberList.Destination);
		}
	}
}