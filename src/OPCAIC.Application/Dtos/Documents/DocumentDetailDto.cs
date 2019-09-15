using System;
using OPCAIC.Application.Dtos.Tournaments;

namespace OPCAIC.Application.Dtos.Documents
{
	public class DocumentDetailDto
	{
		public long Id { get; set; }

		public string Name { get; set; }

		public string Content { get; set; }

		public TournamentReferenceDto Tournament { get; set; }

		public DateTime Created { get; set; }
	}
}