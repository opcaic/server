using OPCAIC.Infrastructure.Enums;

namespace OPCAIC.Infrastructure.Dtos.Tournaments
{
	public class TournamentAuthorizationDto
	{
		public long OwnerId { get; set; }

		public long[] ManagerIds { get; set; }

		public TournamentAvailability Availability { get; set; }
	}
}