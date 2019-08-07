using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using OPCAIC.Infrastructure;

namespace OPCAIC.ApiService.Models.Documents
{
	public class UpdateDocumentModel
	{
		[Required]
		[MinLength(1)]
		[MaxLength(StringLengths.DocumentName)]
		public string Name { get; set; }

		public string Content { get; set; }

		public long TournamentId { get; set; }
	}
}
