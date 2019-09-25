using OPCAIC.Application.Dtos.Users;
using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Application.TournamentInvitations.Models
{
	public class TournamentInvitationDto : IMapFrom<TournamentInvitation>
	{
		public long Id { get; set; }

		public string Email { get; set; }

		public UserReferenceDto User { get; set; }
	}
}