using System.Threading;
using System.Threading.Tasks;
using MediatR;
using OPCAIC.Application.Dtos.EmailTemplates;
using OPCAIC.Application.Extensions;
using OPCAIC.Application.Interfaces;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Application.Specifications;
using OPCAIC.Application.Submissions.Events;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.Emails.EventHandlers
{
	public class SubmissionValidationStateChangedHandler
		: INotificationHandler<SubmissionValidationStateChanged>
	{
		private readonly IEmailService emailService;
		private readonly IFrontendUrlGenerator urlGenerator;
		private readonly IRepository<User> userRepository;

		public SubmissionValidationStateChangedHandler(IEmailService emailService,
			IRepository<User> userRepository, IFrontendUrlGenerator urlGenerator)
		{
			this.emailService = emailService;
			this.userRepository = userRepository;
			this.urlGenerator = urlGenerator;
		}

		/// <inheritdoc />
		public async Task Handle(SubmissionValidationStateChanged notification,
			CancellationToken cancellationToken)
		{
			// notify only on failed
			if (notification.State != SubmissionValidationState.Invalid)
			{
				return;
			}

			var data = await userRepository.GetAsync(notification.SubmissionId,
				s => new Data
				{
					WantsEmailNotifications = s.WantsEmailNotifications, Email = s.Email
				}, cancellationToken);

			if (!data.WantsEmailNotifications)
			{
				return;
			}

			var emailDto =
				new SubmissionValidationFailedEmailDto(
					urlGenerator.SubmissionPageLink(notification.TournamentId,
						notification.SubmissionId));

			await emailService.EnqueueEmailAsync(emailDto, data.Email, cancellationToken);
		}

		public class Data
		{
			public bool WantsEmailNotifications { get; set; }
			public string Email { get; set; }
		}
	}
}