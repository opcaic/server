using System;

namespace OPCAIC.Application.Dtos.Emails
{
	public class EmailResultDto
	{
		public DateTime? SentAt { get; set; }

		public int RemainingAttempts { get; set; }
	}
}