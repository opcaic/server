using System.ComponentModel.DataAnnotations;

namespace OPCAIC.ApiService.Models.Documents
{
	public class DocumentFilterModel : FilterModelBase
	{
		public string Name { get; set; }

		public long? TournamentId { get; set; }
	}
}