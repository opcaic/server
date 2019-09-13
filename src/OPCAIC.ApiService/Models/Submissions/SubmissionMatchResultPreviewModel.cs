using Newtonsoft.Json.Linq;
using OPCAIC.ApiService.Models.Matches;
using OPCAIC.Infrastructure.Enums;

namespace OPCAIC.ApiService.Models.Submissions
{
	public class SubmissionMatchResultPreviewModel
	{
		public SubmissionReferenceModel Submission { get; set; }
		public double Score { get; set; }
		public EntryPointResult CompilerResult { get; set; }
		public JObject AdditionalData { get; set; }
	}
}