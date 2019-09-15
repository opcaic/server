using System.Threading;
using System.Threading.Tasks;
using MediatR;
using OPCAIC.ApiService.Services;
using OPCAIC.Application.Dtos.EmailTemplates;
using OPCAIC.Application.Emails;
using OPCAIC.Application.Interfaces;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Infrastructure.Emails;

namespace OPCAIC.ApiService.Notifications.Events
{
	public class TournamentFinished : INotification
	{
		/// <inheritdoc />
		public TournamentFinished(long tournamentId, string tournamentName)
		{
			TournamentId = tournamentId;
			TournamentName = tournamentName;
		}

		public long TournamentId { get; }
		public string TournamentName { get; }

		public class Handler : INotificationHandler<TournamentFinished>
		{
			private readonly IEmailService emailService;
			private readonly IFrontendUrlGenerator urlGenerator;
			private readonly IUserRepository userRepository;

			public Handler(IEmailService emailService, IUserRepository userRepository, IFrontendUrlGenerator urlGenerator)
			{
				this.emailService = emailService;
				this.userRepository = userRepository;
				this.urlGenerator = urlGenerator;
			}

			/// <inheritdoc />
			public async Task Handle(TournamentFinished notification, CancellationToken cancellationToken)
			{
				var mail = new TournamentFinishedEmailDto
				{
					TournamentName = notification.TournamentName,
					TournamentUrl = urlGenerator.TournamentPageLink(notification.TournamentId)
				};

				var users = await userRepository.GetSubscriberesByTournamentAsync(notification.TournamentId, cancellationToken);

				// TODO: batch email sending
				foreach (var user in users)
				{
					await emailService.EnqueueEmailAsync(mail, user.Email, cancellationToken);
				}
			}
		}
	}
}