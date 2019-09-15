using System.ComponentModel.DataAnnotations;

namespace OPCAIC.Infrastructure.Dtos.Documents
{
	public class UpdateDocumentDto
	{
		public string Name { get; set; }

		public string Content { get; set; }

		public long TournamentId { get; set; }
	}
}