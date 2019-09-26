using System.Linq;
using AutoMapper;
using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Application.Dtos.Matches
{
	public class MatchAuthDto : ICustomMapping
	{
		public long TournamentOwnerId { get; set; }
		public long[] TournamentManagersIds { get; set; }
		public long[] ParticipantsIds { get; set; }
		public bool TournamentPrivateMatchlog { get; set; }

		/// <inheritdoc />
		public void CreateMapping(Profile configuration)
		{
			configuration.CreateMap<Match, MatchAuthDto>(MemberList.Destination)
				.ForMember(m => m.ParticipantsIds,
					opt => opt.MapFrom(m
						=> m.Participations.Select(p => p.Submission.AuthorId).ToArray()))
				.ForMember(m => m.TournamentManagersIds, opt => opt.MapFrom(
					m => m.Tournament.Managers.Select(u => u.UserId).ToArray()));
		}
	}
}