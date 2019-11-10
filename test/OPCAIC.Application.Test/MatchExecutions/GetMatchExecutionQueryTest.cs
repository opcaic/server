using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json.Linq;
using OPCAIC.Application.Dtos.MatchExecutions;
using OPCAIC.Application.Dtos.Submissions;
using OPCAIC.Application.Dtos.Tournaments;
using OPCAIC.Application.Dtos.Users;
using OPCAIC.Application.Infrastructure.Queries;
using OPCAIC.Application.Interfaces;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Application.Matches.Models;
using OPCAIC.Application.MatchExecutions.Models;
using OPCAIC.Application.MatchExecutions.Queries;
using OPCAIC.Application.Specifications;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.Application.Test.MatchExecutions
{
	public class GetMatchExecutionQueryTest : HandlerTest<GetMatchExecutionQuery.Handler>
	{
		/// <inheritdoc />
		public GetMatchExecutionQueryTest(ITestOutputHelper output) : base(output)
		{
			logStorageService = Services.Mock<ILogStorageService>(MockBehavior.Strict);
			matchExecutionRepository =
				Services.Mock<IRepository<MatchExecution>>(MockBehavior.Strict);
			storageService = Services.Mock<IStorageService>(MockBehavior.Strict);
		}

		private readonly Mock<ILogStorageService> logStorageService;
		private readonly Mock<IStorageService> storageService;
		private readonly Mock<IRepository<MatchExecution>> matchExecutionRepository;

		private readonly MatchExecutionDetailDto successfulExecutionPreviewDto = new MatchExecutionDetailDto
		{
			Created = DateTime.Now,
			AdditionalData = new JObject(),
			BotResults = new List<MatchExecutionDetailDto.SubmissionResultDetailDto>
			{
				new MatchExecutionDetailDto.SubmissionResultDetailDto
				{
					AdditionalData = new JObject(),
					CompilerResult = EntryPointResult.Success,
					Score = 1.0,
					Submission = new SubmissionReferenceDto
					{
						Author = new UserReferenceDto
						{

						}
					}
				},
				new MatchExecutionDetailDto.SubmissionResultDetailDto
				{
					AdditionalData = new JObject(),
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
		public async Task Handle_Success()
		{
			matchExecutionRepository.SetupProject(new QueryData<MatchExecutionDetailDto>
			{
				Dto = successfulExecutionPreviewDto,
				OrganizersDto = new TournamentOrganizersDto(),
			}, CancellationToken);

			storageService.Setup(s
					=> s.ReadMatchResultArchive(It.Is<MatchExecutionStorageDto>(d => d.Id == successfulExecutionPreviewDto.Id)))
				.Returns((Stream) null);

			logStorageService
				.Setup(r => r.GetMatchExecutionLogs(
					It.IsAny<MatchExecutionStorageDto>()))
				.Returns(new MatchExecutionLogsDto { ExecutorLog = "log" });

			var dto = await Send<GetMatchExecutionQuery, MatchExecutionDetailDto>(new GetMatchExecutionQuery(successfulExecutionPreviewDto.Id));
				
			dto.BotResults[0].Score.ShouldBe(1.0);
			dto.BotResults[1].Score.ShouldBe(0.0);
			dto.Id.ShouldBe(1);
			dto.ExecutorResult.ShouldBe(EntryPointResult.Success);
		}
	}
}