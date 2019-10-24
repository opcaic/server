using System.Threading;
using System.Threading.Tasks;
using MediatR;
using OPCAIC.ApiService.Users.Commands;
using OPCAIC.Application.Extensions;
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
			private readonly IRepository<TournamentInvitation> invitationRepository;
			private readonly IRepository<TournamentParticipation> participationsRepository;
			private readonly IMediator mediator;

			/// <inheritdoc />
			public Handler(IRepository<TournamentParticipation> participationsRepository,
				IRepository<TournamentInvitation> invitationRepository, IMediator mediator)
			{
				this.participationsRepository = participationsRepository;
				this.invitationRepository = invitationRepository;
				this.mediator = mediator;
			}

			/// <inheritdoc />
			public async Task Handle(UserCreated notification, CancellationToken cancellationToken)
			{
				await mediator.Send(new SendVerificationEmailCommand(notification.User), cancellationToken);

				await AcceptInvitations(notification, cancellationToken);
			}

			private async Task AcceptInvitations(UserCreated notification,
				CancellationToken cancellationToken)
			{
				var invitations =
					await invitationRepository.ListAsync(i => i.Email == notification.User.Email,
						cancellationToken);

				foreach (var invitation in invitations)
				{
					invitation.User = notification.User;

					participationsRepository.Add(new TournamentParticipation
					{
						TournamentId = invitation.TournamentId, UserId = notification.User.Id
					});
				}

				await participationsRepository.SaveChangesAsync(cancellationToken);
				await invitationRepository.SaveChangesAsync(cancellationToken);
			}
		}
	}
}