namespace OPCAIC.Infrastructure.Dtos.Emails
{
	public class NewEmailDto
	{
		public string RecipientEmail { get; set; }

		public string TemplateName { get; set; }

		public string Subject { get; set; }

		public string Body { get; set; }
	}
}