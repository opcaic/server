namespace OPCAIC.Application.Emails.Templates
{
	public partial class EmailType
	{
		public static readonly SubmissionValidationFailedType SubmissionValidationFailed =
			CreateDerived<SubmissionValidationFailedType>();

		public class SubmissionValidationFailedType : EmailType
		{
			public SubmissionValidationFailedType()
				:base(typeof(Email))
			{
				
			}

			public Email CreateEmail(string submissionUrl, string tournamentName)
			{
				return new Email(Name, submissionUrl, tournamentName);
			}

			public class Email : EmailData
			{
				/// <inheritdoc />
				public Email(string templateName, string submissionUrl, string tournamentName)
				{
					TemplateName = templateName;
					SubmissionUrl = submissionUrl;
					TournamentName = tournamentName;
				}

				/// <inheritdoc />
				public override string TemplateName { get; }

				public string SubmissionUrl { get; }

				public string TournamentName { get; }
			}
		}
	}
}