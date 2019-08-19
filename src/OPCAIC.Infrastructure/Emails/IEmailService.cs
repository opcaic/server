using System.Threading;
using System.Threading.Tasks;

namespace OPCAIC.Infrastructure.Emails
{
	public interface IEmailService
	{
		Task SendEmailVerificationEmailAsync(long recipientId, string verificationUrl, 
      CancellationToken cancellationToken);

		Task SendPasswordResetEmailAsync(string recipientEmail, string resetUrl, 
      CancellationToken cancellationToken);

		Task SendTournamentInvitationEmailAsync(string recipientEmail, string tournamentUrl, 
      CancellationToken cancellationToken);
	}
}