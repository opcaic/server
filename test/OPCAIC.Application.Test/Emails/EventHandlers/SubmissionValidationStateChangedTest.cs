using System.Threading.Tasks;
using Moq;
using OPCAIC.Application.Dtos.EmailTemplates;
using OPCAIC.Application.Emails;
using OPCAIC.Application.Emails.EventHandlers;
using OPCAIC.Application.Interfaces;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Application.Specifications;
using OPCAIC.Application.Submissions.Events;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.Application.Test.Emails.EventHandlers
{
	public class SubmissionValidationStateChangedTest : HandlerTest<SubmissionValidationStateChangedHandler>
	{
		private readonly Mock<IEmailService> emailService;
		private readonly Mock<IFrontendUrlGenerator> urlGenerator;
		private readonly Mock<IRepository<User>> userRepository;
		/// <inheritdoc />
		public SubmissionValidationStateChangedTest(ITestOutputHelper output) : base(output)
		{
			emailService = Services.Mock<IEmailService>(MockBehavior.Strict);
			urlGenerator = Services.Mock<IFrontendUrlGenerator>(MockBehavior.Strict);
			userRepository = Services.Mock<IRepository<User>>(MockBehavior.Strict);
		}

		private readonly string email = "a@aaa.com";

		private readonly SubmissionValidationStateChanged Notification =
			new SubmissionValidationStateChanged(1, 2, 3, 4, SubmissionValidationState.Invalid);

		[Theory]
		[InlineData(SubmissionValidationState.Error)]
		[InlineData(SubmissionValidationState.Cancelled)]
		[InlineData(SubmissionValidationState.Queued)]
		public Task DoesNothingForNotFailedValidations(SubmissionValidationState state)
		{
			return Handler.Handle(new SubmissionValidationStateChanged(1, 1, 1, 1, state),
				CancellationToken);
		}

		[Fact]
		public Task DoesNotSendUnwantedNotification()
		{
			var data = new SubmissionValidationStateChangedHandler.Data
			{
				Email = email,
				WantsEmailNotifications = false
			};

			userRepository.SetupProject(data, CancellationToken);

			// nothing more

			return Handler.Handle(Notification, CancellationToken);
		}

		[Fact]
		public Task SendsWantedNotification()
		{
			var data = new SubmissionValidationStateChangedHandler.Data
			{
				Email = email,
				WantsEmailNotifications = true
			};

			userRepository.SetupProject(data, CancellationToken);

			urlGenerator.Setup(g => g.SubmissionPageLink(Notification.TournamentId, Notification.SubmissionId)).Returns("url");

			emailService.Setup(s
				=> s.EnqueueEmailAsync(
					It.Is<SubmissionValidationFailedEmailDto>(d => d.SubmissionUrl == "url"),
					data.Email, CancellationToken)).Returns(Task.CompletedTask);

			return Handler.Handle(Notification, CancellationToken);
		}
	}
}