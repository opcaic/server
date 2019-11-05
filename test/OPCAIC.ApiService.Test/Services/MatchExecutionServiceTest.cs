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
			workerService = Services.Mock<IWorkerService>(MockBehavior.Strict);
			Services.Mock<ISubmissionScoreService>(MockBehavior.Strict);
			Services.Mock<IMatchExecutionRepository>(MockBehavior.Strict);
			Services.Mock<ILogStorageService>(MockBehavior.Strict);
			Services.AddMapper();
			Services.AddSingleton<MatchExecutionService>();
		}

		private readonly Mock<IWorkerService> workerService;

		private readonly string accessToken = "token";
		private readonly string additionalFiles = "files";

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
	}
}