using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OPCAIC.Infrastructure.Dtos.Documents
{
	public class UpdateDocumentDto
	{
		[Required, MinLength(1)]
		public string Name { get; set; }

        public string Content { get; set; }

		public long TournamentId { get; set; }
	}
}
