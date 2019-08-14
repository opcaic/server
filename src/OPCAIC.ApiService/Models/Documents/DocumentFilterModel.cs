using System.ComponentModel.DataAnnotations;

namespace OPCAIC.ApiService.Models.Documents
{
	public class DocumentFilterModel : FilterModelBase
	{
		[MinLength(1)]
		public string Name { get; set; }

		public long? TournamentId { get; set; }
	}
}