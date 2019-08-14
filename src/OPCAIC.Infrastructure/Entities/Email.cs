using System;

namespace OPCAIC.Infrastructure.Entities
{
	public class Email : Entity
	{
		public string RecipientEmail { get; set; }

		public string TemplateName { get; set; }

		public string LastException { get; set; }

		public int RemainingAttempts { get; set; }

		public DateTime? SentAt { get; set; }

		public string Subject { get; set; }

		public string Body { get; set; }
	}
}