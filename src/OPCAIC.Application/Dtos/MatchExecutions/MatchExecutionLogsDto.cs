﻿using System.Collections.Generic;

namespace OPCAIC.Application.Dtos.MatchExecutions
{
	public class MatchExecutionLogsDto
	{
		public List<string> CompilerLogs { get; } = new List<string>();

		public string ExecutorLog { get; set; }
	}
}