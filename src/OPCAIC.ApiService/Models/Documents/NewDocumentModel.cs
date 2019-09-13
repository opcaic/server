namespace OPCAIC.ApiService.Models.Documents
{
	public class NewDocumentModel
	{
		public string Name { get; set; }

		public string Content { get; set; }

		public long TournamentId { get; set; }
	}
}