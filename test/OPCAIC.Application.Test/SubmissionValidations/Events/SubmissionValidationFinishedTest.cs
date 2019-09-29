using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Moq;
using OPCAIC.Application.Dtos.Submissions;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Application.Specifications;
using OPCAIC.Application.Submissions.Events;
using OPCAIC.Application.SubmissionValidations.Events;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.Application.Test.SubmissionValidations.Events
{
	public class SubmissionValidationFinishedTest
		: HandlerTest<SubmissionValidationFinished.Handler>
	{
		/// <inheritdoc />
		public SubmissionValidationFinishedTest(ITestOutputHelper output) : base(output)
		{
			mediator = Services.Mock<IMediator>(MockBehavior.Strict);
			validationRepository =
				Services.Mock<ISubmissionValidationRepository>(MockBehavior.Strict);
			submissionRepository = Services.Mock<ISubmissionRepository>(MockBehavior.Strict);

			// setup common code
			validationRepository.Setup(r
					=> r.UpdateAsync(It.IsAny<ISpecification<SubmissionValidation>>(),
						It.IsAny<SubmissionValidationFinished>(), CancellationToken))
				.ReturnsAsync(true);

			validationRepository.Setup(r
					=> r.FindAsync(
						It.IsAny<IProjectingSpecification<SubmissionValidation,
							SubmissionValidationFinished.Handler.Data>>(), CancellationToken))
				.ReturnsAsync(Data);
		}

		private readonly Mock<IMediator> mediator;
		private readonly Mock<ISubmissionValidationRepository> validationRepository;
		private readonly Mock<ISubmissionRepository> submissionRepository;

		private readonly SubmissionValidationFinished.Handler.Data Data =
			new SubmissionValidationFinished.Handler.Data
			{
				SubmissionId = 1,
				TournamentId = 2,
				UserId = 3,
				ValidationId = 4,
				LastValidationId = 4 // same as ValidationId
			};


		[Fact]
		public Task DoesNothingOnOldValidation()
		{
			Data.LastValidationId = Data.ValidationId + 1; // newer validation exists

			return Handler.Handle(new SubmissionValidationFinished(), CancellationToken);
		}
		
		[Theory]
		[InlineData(EntryPointResult.Success, SubmissionValidationState.Valid)]
		public Task SelectsCorrectValidationState(EntryPointResult validatorResult, SubmissionValidationState state)
		{
			submissionRepository.Setup(r => r.UpdateAsync(It.IsAny<ISpecification<Submission>>(),
					It.Is<UpdateValidationStateDto>(d => d.ValidationState == state),
					CancellationToken))
				.ReturnsAsync(true);

			mediator.Setup(m
					=> m.Publish(It.Is<SubmissionValidationStateChanged>(e => 
						e.SubmissionId == Data.SubmissionId &&
						e.State == state &&
						e.AuthorId == Data.UserId &&
						e.TournamentId == Data.TournamentId &&
						e.ValidationId == Data.ValidationId), CancellationToken))
				.Returns(Task.CompletedTask);

			return Handler.Handle(
				new SubmissionValidationFinished {ValidatorResult = validatorResult,},
				CancellationToken);
		}
	}
}