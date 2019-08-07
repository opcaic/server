using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OPCAIC.ApiService.Models.Documents
{
	public class DocumentFilterModel
	{
		[MinLength(1)]
		public string Name { get; set; }

		public long? TournamentId { get; set; }
	}
}
