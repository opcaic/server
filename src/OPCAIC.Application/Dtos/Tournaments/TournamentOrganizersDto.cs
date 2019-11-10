using System;
using System.Linq;
using AutoMapper;
using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.Dtos.Tournaments
{
	public class TournamentOrganizersDto : ICustomMapping
	{
		public long OwnerId { get; set; }
		public long[] ManagersIds { get; set; } = Array.Empty<long>();

		public bool IsOrganizer(long? userId, UserRole role = UserRole.None)
		{
			return role == UserRole.Admin || 
				userId.HasValue && (OwnerId == userId || ManagersIds.Contains(userId.Value));
		}

		/// <inheritdoc />
		void ICustomMapping.CreateMapping(Profile configuration)
		{
			configuration.CreateMap<Tournament, TournamentOrganizersDto>(MemberList.Destination)
				.ForMember(m => m.ManagersIds,
					opt => opt.MapFrom(t => t.Managers.Select(m => m.UserId)));
		}
	}
}