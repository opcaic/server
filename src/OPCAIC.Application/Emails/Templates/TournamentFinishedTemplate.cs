namespace OPCAIC.Application.Emails.Templates
{
	public partial class EmailType
	{
		public static readonly TournamentFinishedType TournamentFinished =
			CreateDerived<TournamentFinishedType>();

		public class TournamentFinishedType : EmailType
		{
			public TournamentFinishedType()
				: base(typeof(Email))
			{
				
			}

			public Email CreateEmail(string tournamentUrl, string tournamentName)
			{
				return new Email(Name, tournamentUrl, tournamentName);
			}

			public class Email : EmailData
			{
				/// <inheritdoc />
				public Email(string templateName, string tournamentUrl, string tournamentName)
				{
					TemplateName = templateName;
					TournamentUrl = tournamentUrl;
					TournamentName = tournamentName;
				}

				/// <inheritdoc />
				public override string TemplateName { get; }

				public string TournamentUrl { get; }

				public string TournamentName { get; }
			}
		}
	}
}