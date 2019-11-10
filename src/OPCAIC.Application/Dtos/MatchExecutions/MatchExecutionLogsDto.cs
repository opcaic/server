using System.Collections.Generic;

namespace OPCAIC.Application.Dtos.MatchExecutions
{
	public class MatchExecutionLogsDto
	{
		public class SubmissionLog
		{
			public string CompilerLog { get; set; }
			public string CompilerErrorLog { get; set; }
		}

		public List<SubmissionLog> SubmissionLogs { get; } = new List<SubmissionLog>();

		public string ExecutorLog { get; set; }
		public string ExecutorErrorLog { get; set; }
	}
}