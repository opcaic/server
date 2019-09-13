namespace OPCAIC.ApiService.Models.Documents
{
	public class UpdateDocumentModel
	{
		public string Name { get; set; }

		public string Content { get; set; }

		public long TournamentId { get; set; }
	}
}