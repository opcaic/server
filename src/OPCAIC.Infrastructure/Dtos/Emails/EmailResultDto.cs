﻿using System;

namespace OPCAIC.Infrastructure.Dtos.Emails
{
	public class EmailResultDto
	{
		public DateTime? SentAt { get; set; }

		public Exception LastException { get; set; }

		public int RemainingAttempts { get; set; }
	}
}
