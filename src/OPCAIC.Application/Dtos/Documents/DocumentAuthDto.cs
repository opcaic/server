namespace OPCAIC.Application.Dtos.Documents
{
	public class DocumentAuthDto
	{
		public long TournamentOwnerId { get; set; }

		public long[] TournamentManagersIds { get; set; }
	}
}