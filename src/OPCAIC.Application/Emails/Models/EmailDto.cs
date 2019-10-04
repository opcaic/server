using System;
using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Application.Emails.Models
{
	public class EmailDto : IMapFrom<Email>
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