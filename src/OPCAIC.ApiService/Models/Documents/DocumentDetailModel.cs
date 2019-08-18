using OPCAIC.ApiService.Models.Tournaments;

namespace OPCAIC.ApiService.Models.Documents
{
	public class DocumentDetailModel
	{
		public long Id { get; set; }

		public string Name { get; set; }

		public string Content { get; set; }

		public TournamentReferenceModel Tournament { get; set; }
	}
}