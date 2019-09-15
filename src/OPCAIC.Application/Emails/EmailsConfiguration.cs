namespace OPCAIC.Application.Emails
{
	public class EmailsConfiguration
	{
		public string SmtpServerUrl { get; set; }

		public string UserName { get; set; }

		public string Password { get; set; }

		public bool UseSsl { get; set; }

		public int Port { get; set; }

		public string SenderAddress { get; set; }
	}
}