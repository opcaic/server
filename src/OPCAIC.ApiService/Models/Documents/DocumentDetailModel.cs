using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OPCAIC.ApiService.Models.Documents
{
	public class DocumentDetailModel
	{
		public long Id { get; set; }

		public string Name { get; set; }

		public TournamentReferenceModel Tournament { get; set; }
	}
}
