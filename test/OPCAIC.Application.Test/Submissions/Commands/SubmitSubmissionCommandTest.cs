using System;
using System.IO;
using System.Threading.Tasks;
using MediatR;
using Moq;
using OPCAIC.ApiService.IoC;
using OPCAIC.Application.Dtos.Base;
using OPCAIC.Application.Dtos.Submissions;
using OPCAIC.Application.Exceptions;
using OPCAIC.Application.Infrastructure.Validation;
using OPCAIC.Application.Interfaces;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Application.Specifications;
using OPCAIC.Application.Submissions.Commands;
using OPCAIC.Application.Submissions.Events;
using OPCAIC.Application.Tournaments.Models;
using OPCAIC.Common;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.Application.Test.Submissions.Commands
{
	public class SubmitSubmissionCommandTest : HandlerTest<SubmitSubmissionCommand.Handler>
	{
		/// <inheritdoc />
		public SubmitSubmissionCommandTest(ITestOutputHelper output) : base(output)
		{
			Services.AddMapper();
			submissionRepository = Services.Mock<ISubmissionRepository>(MockBehavior.Strict);
			tournamentRepository = Services.Mock<ITournamentRepository>(MockBehavior.Strict);
			tournamentParticipationRepository =
				Services.Mock<IRepository<TournamentParticipation>>(MockBehavior.Strict);
			storageService = Services.Mock<IStorageService>(MockBehavior.Strict);
			time = Services.Mock<ITimeService>(MockBehavior.Strict);
			mediator = Services.Mock<IMediator>(MockBehavior.Strict);

			time.SetupGet(s => s.Now).Returns(DateTime.Now);

			Faker.Configure<Tournament>()
				.RuleFor(t => t.Id, TournamentId)
				.RuleFor(d => d.State, TournamentState.Published);

			TournamentParticipation = new TournamentParticipation
			{
				TournamentId = TournamentId,
				UserId = UserId
			};
		}

		private readonly Mock<ISubmissionRepository> submissionRepository;
		private readonly Mock<ITournamentRepository> tournamentRepository;

		private readonly Mock<IRepository<TournamentParticipation>>
			tournamentParticipationRepository;

		private readonly Mock<IStorageService> storageService;
		private readonly Mock<ITimeService> time;
		private readonly Mock<IMediator> mediator;

		public long TournamentId = 1;
		public long SubmissionId = 1;
		public long UserId = 1;

		public TournamentParticipation TournamentParticipation;

		[Fact]
		public async Task Create_AfterDeadline()
		{
			Faker.Configure<Tournament>()
				.RuleFor(d => d.Deadline, DateTime.Now - TimeSpan.FromHours(2));

			tournamentRepository.Setup(r => r.FindByIdAsync(TournamentId, CancellationToken))
				.ReturnsAsync(Faker.Dto<Tournament, TournamentDetailDto>());

			var exception = await Should.ThrowAsync<BusinessException>(()
				=> Handler.Handle(
					new SubmitSubmissionCommand
					{
						TournamentId = TournamentId, RequestingUserId = UserId
					}, CancellationToken));

			exception.ErrorCode.ShouldBe(ValidationErrorCodes.TournamentDeadlinePassed);
		}

		[Fact]
		public async Task Create_FinishedTournament()
		{
			Faker.Configure<Tournament>()
				.RuleFor(d => d.Deadline, DateTime.Now - TimeSpan.FromHours(2));

			tournamentRepository.Setup(r => r.FindByIdAsync(TournamentId, CancellationToken))
				.ReturnsAsync(Faker.Dto<Tournament, TournamentDetailDto>());

			var exception = await Should.ThrowAsync<BusinessException>(()
				=> Handler.Handle(
					new SubmitSubmissionCommand
					{
						TournamentId = TournamentId, RequestingUserId = UserId
					}, CancellationToken));

			exception.ErrorCode.ShouldBe(ValidationErrorCodes.TournamentDeadlinePassed);
		}

		[Fact]
		public async Task Create_Success_FirstTime()
		{
			tournamentRepository.Setup(r => r.FindByIdAsync(TournamentId, CancellationToken))
				.ReturnsAsync(Faker
					.Dto<Tournament, TournamentDetailDto>()); // not deadline by default

			submissionRepository.Setup(s => 
				s.Add(It.Is<Submission>(s => 
					s.TournamentId == TournamentId &&
					s.AuthorId == UserId &&
					s.TournamentParticipation.TournamentId == TournamentId)))
				.Callback((Submission s) => s.Id = SubmissionId);

			submissionRepository.Setup(r
					=> r.FindSubmissionForStorageAsync(SubmissionId, CancellationToken))
				.ReturnsAsync(new SubmissionDtoBase {Id = SubmissionId});

			submissionRepository.Setup(r => r.SaveChangesAsync(CancellationToken))
				.Returns(Task.CompletedTask);

			storageService.Setup(s => s.WriteSubmissionArchive(
					It.Is<SubmissionDtoBase>(d => d.Id == SubmissionId)))
				.Returns(new MemoryStream());

			tournamentParticipationRepository.SetupFind(null, CancellationToken);
			tournamentParticipationRepository.Setup(s
				=> s.Add(It.Is<TournamentParticipation>(p
					=> p.UserId == UserId && p.TournamentId == TournamentId)));

			mediator.Setup(m
				=> m.Publish(It.Is<SubmissionCreated>(e => e.SubmissionId == SubmissionId),
					CancellationToken)).Returns(Task.CompletedTask);

			var id = await Handler.Handle(
				new SubmitSubmissionCommand
				{
					TournamentId = TournamentId,
					Archive = new MemoryStream(),
					RequestingUserId = UserId
				}, CancellationToken);

			id.ShouldBe(SubmissionId);
		}

		[Fact]
		public async Task Create_Success_SecondTime()
		{
			tournamentRepository.Setup(r => r.FindByIdAsync(TournamentId, CancellationToken))
				.ReturnsAsync(Faker
					.Dto<Tournament, TournamentDetailDto>()); // not deadline by default

			submissionRepository.Setup(s =>
					s.Add(It.Is<Submission>(s =>
						s.TournamentId == TournamentId &&
						s.AuthorId == UserId &&
						s.TournamentParticipation == TournamentParticipation)))
				.Callback((Submission s) => s.Id = SubmissionId);

			submissionRepository.Setup(r
					=> r.FindSubmissionForStorageAsync(SubmissionId, CancellationToken))
				.ReturnsAsync(new SubmissionDtoBase {Id = SubmissionId});

			submissionRepository.Setup(r => r.SaveChangesAsync(CancellationToken))
				.Returns(Task.CompletedTask);

			storageService.Setup(s => s.WriteSubmissionArchive(
					It.Is<SubmissionDtoBase>(d => d.Id == SubmissionId)))
				.Returns(new MemoryStream());

			tournamentParticipationRepository.SetupFind(TournamentParticipation, CancellationToken);

			mediator.Setup(m
				=> m.Publish(It.Is<SubmissionCreated>(e => e.SubmissionId == SubmissionId),
					CancellationToken)).Returns(Task.CompletedTask);

			var id = await Handler.Handle(
				new SubmitSubmissionCommand
				{
					TournamentId = TournamentId,
					Archive = new MemoryStream(),
					RequestingUserId = UserId
				}, CancellationToken);

			id.ShouldBe(SubmissionId);
		}
	}
}