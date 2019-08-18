using System.ComponentModel.DataAnnotations;
using OPCAIC.Infrastructure;

namespace OPCAIC.ApiService.Models.Documents
{
	public class UpdateDocumentModel
	{
		[Required]
		[MaxLength(StringLengths.DocumentName)]
		public string Name { get; set; }

		public string Content { get; set; }

		public long TournamentId { get; set; }
	}
}