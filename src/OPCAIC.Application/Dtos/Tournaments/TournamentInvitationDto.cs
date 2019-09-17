using OPCAIC.Application.Dtos.Users;

namespace OPCAIC.Application.Dtos.Tournaments
{
	public class TournamentInvitationDto
	{
		public long Id { get; set; }

		public string Email { get; set; }

		public UserReferenceDto User { get; set; }
	}
}