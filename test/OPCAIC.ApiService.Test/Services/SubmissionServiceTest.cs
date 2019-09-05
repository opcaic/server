using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Internal;
using Moq;
using OPCAIC.ApiService.Exceptions;
using OPCAIC.ApiService.IoC;
using OPCAIC.ApiService.Models.Submissions;
using OPCAIC.ApiService.ModelValidationHandling;
using OPCAIC.ApiService.Services;
using OPCAIC.Infrastructure.Dtos;
using OPCAIC.Infrastructure.Dtos.Submissions;
using OPCAIC.Infrastructure.Dtos.Tournaments;
using OPCAIC.Infrastructure.Dtos.Users;
using OPCAIC.Infrastructure.Entities;
using OPCAIC.Infrastructure.Enums;
using OPCAIC.Infrastructure.Repositories;
using OPCAIC.Services;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.ApiService.Test.Services
{
	public class SubmissionServiceTest : ApiServiceTestBase
	{
		/// <inheritdoc />
		public SubmissionServiceTest(ITestOutputHelper output) : base(output)
		{
			Services.AddMapper();
			submissionRepository = Services.Mock<ISubmissionRepository>(MockBehavior.Strict);
			tournamentRepository = Services.Mock<ITournamentRepository>(MockBehavior.Strict);
			storageService = Services.Mock<IStorageService>(MockBehavior.Strict);

			Faker.Configure<Tournament>()
				.RuleFor(t => t.Id, TournamentId)
				.RuleFor(d => d.State, TournamentState.Published);
		}

		private readonly Mock<ISubmissionRepository> submissionRepository;
		private readonly Mock<ITournamentRepository> tournamentRepository;
		private readonly Mock<IStorageService> storageService;

		public long TournamentId = 1;
		public long SubmissionId = 1;
		public long UserId = 1;

		private SubmissionService SubmissionService => GetService<SubmissionService>();

		[Fact]
		public async Task Create_AfterDeadline()
		{
			Faker.Configure<Tournament>()
				.RuleFor(d => d.Deadline, DateTime.Now - TimeSpan.FromHours(2));

			tournamentRepository.Setup(r => r.FindByIdAsync(TournamentId, CancellationToken))
				.ReturnsAsync(Faker.Dto<Tournament, TournamentDetailDto>());

			var exception = await Should.ThrowAsync<BadRequestException>(()
				=> SubmissionService.CreateAsync(
					new NewSubmissionModel {TournamentId = TournamentId}, 0, CancellationToken));

			var error = exception.ValidationErrors.ShouldHaveSingleItem();
			error.Code.ShouldBe(ValidationErrorCodes.TournamentDeadlinePassed);
		}

		[Fact]
		public async Task Create_FinishedTournament()
		{
			Faker.Configure<Tournament>()
				.RuleFor(d => d.Deadline, DateTime.Now - TimeSpan.FromHours(2));

			tournamentRepository.Setup(r => r.FindByIdAsync(TournamentId, CancellationToken))
				.ReturnsAsync(Faker.Dto<Tournament, TournamentDetailDto>());

			var exception = await Should.ThrowAsync<BadRequestException>(()
				=> SubmissionService.CreateAsync(
					new NewSubmissionModel {TournamentId = TournamentId}, 0, CancellationToken));

			var error = exception.ValidationErrors.ShouldHaveSingleItem();
			error.Code.ShouldBe(ValidationErrorCodes.TournamentDeadlinePassed);
		}

		[Fact]
		public async Task Create_Success()
		{
			tournamentRepository.Setup(r => r.FindByIdAsync(TournamentId, CancellationToken))
				.ReturnsAsync(Faker.Dto<Tournament, TournamentDetailDto>()); // not deadline by default

			submissionRepository.Setup(r => r.CreateAsync(It.Is<NewSubmissionDto>(
					d => d.AuthorId == UserId), CancellationToken))
				.ReturnsAsync(SubmissionId);

			submissionRepository.Setup(r
					=> r.FindSubmissionForStorageAsync(SubmissionId, CancellationToken))
				.ReturnsAsync(new SubmissionStorageDto {Id = SubmissionId});

			storageService.Setup(s => s.WriteSubmissionArchive(
					It.Is<SubmissionStorageDto>(d => d.Id == SubmissionId)))
				.Returns(new MemoryStream());

			var id = await SubmissionService.CreateAsync(
				new NewSubmissionModel
				{
					TournamentId = TournamentId,
					Archive = new FormFile(new MemoryStream(), 0, 0, "a.zip", "a.zip")
				}, UserId,
				CancellationToken);

			id.ShouldBe(SubmissionId);
		}

		[Fact]
		public async Task GetByFilter_Sucess()
		{
			submissionRepository.Setup(r
					=> r.GetByFilterAsync(It.IsAny<SubmissionFilterDto>(), CancellationToken))
				.ReturnsAsync(new ListDto<SubmissionPreviewDto>
				{
					Total = 1,
					List = new List<SubmissionPreviewDto>
					{
						new SubmissionPreviewDto
						{
							Tournament = new TournamentReferenceDto {Id = TournamentId},
							Author = new UserReferenceDto {Id = UserId}
						}
					}
				});

			var list = await SubmissionService
				.GetByFilterAsync(new SubmissionFilterModel {Count = 1}, CancellationToken);

			list.Total.ShouldBe(1);
			list.List.ShouldHaveSingleItem();
		}
	}
}