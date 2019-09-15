namespace OPCAIC.Application.Dtos.Matches
{
	public class MatchAuthDto
	{
		public long TournamentOwnerId { get; set; }
		public long[] TournamentManagersIds { get; set; }
		public long[] ParticipantsIds { get; set; }
	}
}