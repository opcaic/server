using System.Threading.Tasks;
using Moq;
using OPCAIC.Application.Dtos.TournamentParticipations;
using OPCAIC.Application.Specifications;
using OPCAIC.Application.Submissions.Events;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.Application.Test.Submissions.Events
{
	public class SubmissionValidationStateChangedTest
		: HandlerTest<SubmissionValidationStateChanged.Handler>
	{
		/// <inheritdoc />
		public SubmissionValidationStateChangedTest(ITestOutputHelper output) : base(output)
		{
			pariticipationsRepository =
				Services.Mock<IRepository<TournamentParticipation>>(MockBehavior.Strict);
			repository = Services.Mock<IRepository<Submission>>(MockBehavior.Strict);
		}

		private readonly Mock<IRepository<TournamentParticipation>> pariticipationsRepository;
		private readonly Mock<IRepository<Submission>> repository;

		private readonly SubmissionValidationStateChanged Notification =
			new SubmissionValidationStateChanged(1, 2, 3, 4, "tournament", SubmissionValidationState.Valid);

		private readonly SubmissionValidationStateChanged.Handler.Data Data =
			new SubmissionValidationStateChanged.Handler.Data {LastSubmissionId = 1};

		[Fact]
		public Task DoesNotUpdateActiveSubmissionWhenNewerExists()
		{
			Data.LastSubmissionId = Notification.SubmissionId + 1; // newer submission

			repository.SetupProject(Data, CancellationToken);

			// nothing more

			return Handler.Handle(Notification, CancellationToken);
		}

		[Fact]
		public Task IgnoresInvalidStates()
		{
			// strict mocks handle the logic
			return Handler.Handle(
				new SubmissionValidationStateChanged(1, 2, 3, 4, "tournament", SubmissionValidationState.Invalid),
				CancellationToken);
		}

		[Fact]
		public async Task UpdatesActiveSubmission()
		{
			Data.LastSubmissionId = Notification.SubmissionId;

			repository.SetupProject(Data, CancellationToken);
			var participation = new TournamentParticipation();
			pariticipationsRepository.SetupFind(participation, CancellationToken);
			pariticipationsRepository.Setup(s => s.SaveChangesAsync(CancellationToken))
				.Returns(Task.CompletedTask);

			await Handler.Handle(Notification, CancellationToken);

			participation.ActiveSubmissionId.ShouldBe(Notification.SubmissionId);
		}
	}
}