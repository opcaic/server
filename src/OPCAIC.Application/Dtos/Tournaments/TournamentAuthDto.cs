using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.Dtos.Tournaments
{
	public class TournamentAuthDto
	{
		public long Id { get; set; }

		public long OwnerId { get; set; }

		public long[] ManagerIds { get; set; }

		public TournamentAvailability Availability { get; set; }
	}
}