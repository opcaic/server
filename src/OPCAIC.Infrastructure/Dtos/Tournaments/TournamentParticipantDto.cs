using OPCAIC.Infrastructure.Dtos.Users;

namespace OPCAIC.Infrastructure.Dtos.Tournaments
{
	public class TournamentParticipantDto
	{
		public long Id { get; set; }

		public string Email { get; set; }

		public UserReferenceDto User { get; set; }
	}
}
