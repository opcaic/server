using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using OPCAIC.ApiService.Models.Submissions;
using OPCAIC.Infrastructure.Enums;

namespace OPCAIC.ApiService.Models.Matches
{
	public class MatchExecutionPreviewModel
	{
		public long Id { get; set; }
		public MatchReferenceModel Match { get; set; }
		public EntryPointResult ExecutorResult { get; set; }
		public IList<SubmissionMatchResultPreviewModel> BotResults { get; set; }
		public DateTime? Executed { get; set; }
		public DateTime Created { get; set; }
		public JObject AdditionalData { get; set; }
	}

	public class MatchExecutionDetailModel : MatchExecutionPreviewModel
	{
		public string ExecutorLog { get; set; }

		// Bot specific logs will be added to base.BotResults[i] via
		// SubmissionMatchResultDetailModel (deriving from *PreviewModel)
	}
}