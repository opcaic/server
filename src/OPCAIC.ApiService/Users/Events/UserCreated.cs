using System.Threading;
using System.Threading.Tasks;
using MediatR;
using OPCAIC.ApiService.Interfaces;
using OPCAIC.Application.Dtos.EmailTemplates;
using OPCAIC.Application.Emails;
using OPCAIC.Application.Extensions;
using OPCAIC.Application.Interfaces;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Application.Specifications;
using OPCAIC.Domain.Entities;

namespace OPCAIC.ApiService.Users.Events
{
	public class UserCreated : INotification
	{
		public UserCreated(User user)
		{
			User = user;
		}

		public User User { get; }

		public class Handler : INotificationHandler<UserCreated>
		{
			private readonly IUserManager userManager;
			private readonly IFrontendUrlGenerator urlGenerator;
			private readonly IEmailService emailService;
			private readonly IRepository<TournamentParticipation> participationsRepository;
			private readonly ITournamentInvitationRepository invitationRepository;

			/// <inheritdoc />
			public Handler(IUserManager userManager, IFrontendUrlGenerator urlGenerator, IEmailService emailService, IRepository<TournamentParticipation> participationsRepository, ITournamentInvitationRepository invitationRepository)
			{
				this.userManager = userManager;
				this.urlGenerator = urlGenerator;
				this.emailService = emailService;
				this.participationsRepository = participationsRepository;
				this.invitationRepository = invitationRepository;
			}

			/// <inheritdoc />
			public async Task Handle(UserCreated notification, CancellationToken cancellationToken)
			{
				await SendVerificationEmail(notification, cancellationToken);

				await AcceptInvitations(notification, cancellationToken);
			}

			private async Task AcceptInvitations(UserCreated notification, CancellationToken cancellationToken)
			{
				var invitations =
					await invitationRepository.ListAsync(i => i.Email == notification.User.Email,
						cancellationToken);

				foreach (var invitation in invitations)
				{
					invitation.User = notification.User;

					participationsRepository.Add(new TournamentParticipation
					{
						TournamentId = invitation.TournamentId,
						UserId = notification.User.Id
					});
				}

				await participationsRepository.SaveChangesAsync(cancellationToken);
				await invitationRepository.SaveChangesAsync(cancellationToken);
			}

			private async Task SendVerificationEmail(UserCreated notification,
				CancellationToken cancellationToken)
			{
				var token = await userManager.GenerateEmailConfirmationTokenAsync(notification.User);
				var url = urlGenerator.EmailConfirmLink(notification.User.Email, token);

				await emailService.EnqueueEmailAsync(
					new UserVerificationEmailDto(url), notification.User.Email,
					cancellationToken);
			}
		}
	}
}