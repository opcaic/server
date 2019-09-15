﻿using System;

namespace OPCAIC.Application.Dtos.Emails
{
	public class EmailPreviewDto
	{
		public long Id { get; set; }

		public string RecipientEmail { get; set; }

		public string TemplateName { get; set; }

		public int RemainingAttempts { get; set; }

		public DateTime? SentAt { get; set; }

		public string Subject { get; set; }

		public string Body { get; set; }
	}
}