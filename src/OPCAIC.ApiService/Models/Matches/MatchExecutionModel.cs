using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using OPCAIC.ApiService.Models.Submissions;
using OPCAIC.Infrastructure.Enums;

namespace OPCAIC.ApiService.Models.Matches
{
	public class MatchExecutionModel
	{
		public MatchReferenceModel Match { get; set; }
		public EntryPointResult ExecutorResult { get; set; }
		public IList<SubmissionMatchResultModel> BotResults { get; set; }
		public DateTime? Executed { get; set; }
		public DateTime Created { get; set; }
		public JObject AdditionalData { get; set; }
	}
}