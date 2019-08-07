using System;
using System.Collections.Generic;
using System.Text;

namespace OPCAIC.Infrastructure.Dtos.Documents
{
	public class DocumentDetailDto
	{
		public long Id { get; set; }

		public string Name { get; set; }

		public TournamentReferenceDto Tournament { get; set; }

		public DateTime Created { get; set; }
	}
}
