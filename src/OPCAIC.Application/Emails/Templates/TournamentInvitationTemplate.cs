namespace OPCAIC.Application.Emails.Templates
{
	public partial class EmailType
	{
		public static readonly TournamentInvitationType TournamentInvitation =
			CreateDerived<TournamentInvitationType>();

		public class TournamentInvitationType : EmailType
		{
			public TournamentInvitationType()
				: base(typeof(Email))
			{

			}

			public Email CreateEmail(string tournamentUrl, string tournamentName, string userName)
			{
				return new Email(Name, tournamentUrl, tournamentName, userName);
			}

			public class Email : EmailData
			{
				/// <inheritdoc />
				public Email(string templateName, string tournamentUrl, string tournamentName, string userName)
				{
					TemplateName = templateName;
					TournamentUrl = tournamentUrl;
					TournamentName = tournamentName;
					UserName = userName;
				}

				/// <inheritdoc />
				public override string TemplateName { get; }

				public string TournamentUrl { get; }

				public string TournamentName { get; }

				public string UserName { get; }
			}
		}
	}
}