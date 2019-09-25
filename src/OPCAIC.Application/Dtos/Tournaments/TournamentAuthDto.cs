using System.Linq;
using AutoMapper;
using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.Dtos.Tournaments
{
	public class TournamentAuthDto : ICustomMapping
	{
		public long Id { get; set; }

		public long OwnerId { get; set; }

		public long[] ManagerIds { get; set; }

		public long[] ParticipantIds { get; set; }

		public TournamentAvailability Availability { get; set; }

		/// <inheritdoc />
		void ICustomMapping.CreateMapping(Profile configuration)
		{
			configuration.CreateMap<Tournament, TournamentAuthDto>(MemberList.Destination)
				.ForMember(d => d.ManagerIds,
					opt => opt.MapFrom(s => s.Managers.Select(m => m.UserId)))
				.ForMember(d => d.ParticipantIds,
					opt => opt.MapFrom(e => e.Participants.Select(s => s.UserId)));

		}
	}
}