using Microsoft.Extensions.DependencyInjection;
using Moq;
using OPCAIC.ApiService.IoC;
using OPCAIC.ApiService.Services;
using OPCAIC.Application.Dtos.MatchExecutions;
using OPCAIC.Application.Dtos.Submissions;
using OPCAIC.Application.Dtos.Users;
using OPCAIC.Application.Interfaces;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Domain.Enums;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Common;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.ApiService.Test.Services
{
	public class MatchExecutionServiceTest : ApiServiceTestBase
	{
		/// <inheritdoc />
		public MatchExecutionServiceTest(ITestOutputHelper output) : base(output)
		{
			logStorageService = Services.Mock<ILogStorageService>(MockBehavior.Strict);
			workerService = Services.Mock<IWorkerService>(MockBehavior.Strict);
			matchExecutionRepository =
				Services.Mock<IMatchExecutionRepository>(MockBehavior.Strict);
			submissionScoreService = Services.Mock<ISubmissionScoreService>(MockBehavior.Strict);
			Services.AddMapper();
			Services.AddSingleton<MatchExecutionService>();
		}

		private readonly Mock<ILogStorageService> logStorageService;
		private readonly Mock<IMatchExecutionRepository> matchExecutionRepository;
		private readonly Mock<IWorkerService> workerService;
		private readonly Mock<ISubmissionScoreService> submissionScoreService;

		private readonly string accessToken = "token";
		private readonly string additionalFiles = "files";

		private readonly MatchExecutionDto successfulExecutionDto = new MatchExecutionDto
		{
			Created = DateTime.Now,
			AdditionalData = "{}",
			BotResults = new List<SubmissionMatchResultDto>
			{
				new SubmissionMatchResultDto
				{
					AdditionalData = "{}",
					CompilerResult = EntryPointResult.Success,
					Score = 1.0,
					Submission = new SubmissionReferenceDto
					{
						Author = new UserReferenceDto
						{

						}
					}
				},
				new SubmissionMatchResultDto
				{
					AdditionalData = "{}",
					CompilerResult = EntryPointResult.Success,
					Score = 0.0,
					Submission = new SubmissionReferenceDto
					{
						Author = new UserReferenceDto
						{

						}
					}
				}
			},
			ExecutorResult = EntryPointResult.Success,
			Id = 1
		};

		[Fact]
		public async Task CreateRequest_Success()
		{
			workerService
				.Setup(r => r.GetAdditionalFilesUrl(It.IsAny<long>()))
				.Returns(additionalFiles);
			workerService
				.Setup(r => r.GenerateWorkerToken(It.IsAny<ClaimsIdentity>()))
				.Returns(accessToken);

			var request = GetService<MatchExecutionService>()
				.CreateRequest(new MatchExecutionRequestDataDto
				{
					Id = 100,
					SubmissionIds = new List<long> { 1, 0 }
				});

			request.AccessToken.ShouldBe(accessToken);
			request.AdditionalFilesUri.ShouldBe(additionalFiles);
			request.ExecutionId.ShouldBe(100);
		}

		[Fact]
		public async Task GetById_Success()
		{
			matchExecutionRepository
				.Setup(r => r.FindByIdAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(successfulExecutionDto);

			logStorageService
				.Setup(r => r.GetMatchExecutionLogs(
					It.IsAny<MatchExecutionStorageDto>()))
				.Returns(new MatchExecutionLogsDto { ExecutorLog = "log" });

			var model = await GetService<MatchExecutionService>()
				.GetByIdAsync(0, CancellationToken);
			model.BotResults[0].Score.ShouldBe(1.0);
			model.BotResults[1].Score.ShouldBe(0.0);
			model.Id.ShouldBe(1);
			model.ExecutorResult.ShouldBe(EntryPointResult.Success);
		}
	}
}