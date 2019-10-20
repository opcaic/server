using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using OPCAIC.Application.Dtos.Users;
using OPCAIC.Application.Emails;
using OPCAIC.Application.Emails.Templates;
using OPCAIC.Application.Interfaces;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Application.Extensions;
using OPCAIC.Application.Specifications;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Application.Tournaments.Events
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
			private readonly IMapper mapper;
			private readonly IEmailService emailService;
			private readonly IFrontendUrlGenerator urlGenerator;
			private readonly IRepository<User> userRepository;

			public Handler(IEmailService emailService, IRepository<User> userRepository, IFrontendUrlGenerator urlGenerator, IMapper mapper)
			{
				this.emailService = emailService;
				this.userRepository = userRepository;
				this.urlGenerator = urlGenerator;
				this.mapper = mapper;
			}

			/// <inheritdoc />
			public async Task Handle(TournamentFinished notification, CancellationToken cancellationToken)
			{
				var data = EmailType.TournamentFinished.CreateEmail(
					 urlGenerator.TournamentPageLink(notification.TournamentId),
					 notification.TournamentName
				);

				var userEmails = await userRepository.ListAsync(u
					=> u.WantsEmailNotifications &&
					u.Submissions.Any(s => s.TournamentId == notification.TournamentId), u => u.Email, cancellationToken);

				// TODO: batch email sending?
				foreach (var email in userEmails)
				{
					await emailService.EnqueueEmailAsync(data, email, cancellationToken);
				}
			}
		}
	}
}