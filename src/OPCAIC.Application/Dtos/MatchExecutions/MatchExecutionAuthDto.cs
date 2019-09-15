namespace OPCAIC.Application.Dtos.MatchExecutions
{
	public class MatchExecutionAuthDto
	{
		public long Id { get; set; }
		public long TournamentOwnerId { get; set; }
		public long[] TournamentManagersIds { get; set; }
		public long[] MatchParticipantsUserIds { get; set; }
	}
}