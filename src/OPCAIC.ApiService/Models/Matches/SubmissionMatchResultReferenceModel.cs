using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OPCAIC.ApiService.Models.Matches
{
	public class SubmissionMatchResultReferenceModel
	{
		public long SubmissionId { get; set; }
		public double Score { get; set; }
		public string AdditionalDataJson { get; set; }
	}
}
