namespace OPCAIC.Infrastructure.Dtos.Documents
{
	public class NewDocumentDto
	{
		public string Name { get; set; }
		public string Content { get; set; }
		public long TournamentId { get; set; }
	}
}