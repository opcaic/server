using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using OPCAIC.GameModules.Interface;
using OPCAIC.Messaging.Messages;
using OPCAIC.Worker.Services;
using Xunit;
using Xunit.Abstractions;
using BotInfo = OPCAIC.Messaging.Messages.BotInfo;
using BotResult = OPCAIC.Messaging.Messages.BotResult;

namespace OPCAIC.Worker.Test
{
	public class MatchExecutorTest : JobExecutorTest
	{
		/// <inheritdoc />
		public MatchExecutorTest(ITestOutputHelper output) : base(output)
		{
			Request = new MatchExecutionRequest
			{
				JobId = Guid.NewGuid(),
				Game = "",
				ExecutionId = 1,
				Bots = new List<BotInfo>
				{
					new BotInfo {SubmissionId = 1}, new BotInfo {SubmissionId = 2}
				},
				AccessToken = "",
				Configuration = "{}"
			};
		}

		private MatchExecutionRequest Request { get; }

		private MatchExecutor MatchExecutor => GetService<MatchExecutor>();

		private static MatchResult ToSuccessMatchResult(MatchExecutionRequest request)
		{
			var i = 0;
			return new MatchResult
			{
				AdditionalInfo = new Dictionary<string,
					object>(),
				Results = request.Bots.Select(b => new OPCAIC.GameModules.Interface.BotResult
				{
					Score = i++,
					AdditionalInfo = new Dictionary<string, object>(),
					HasCrashed = false
				}).ToArray()
			};
		}


		private static void AssertResultSuccess(MatchExecutionResult result)
		{
			Assert.Equal(JobStatus.Ok, result.JobStatus);
			Assert.Equal(SubTaskResult.Ok, result.ExecutionResult);
			Assert.NotNull(result.AdditionalData);
			Assert.NotNull(result.BotResults);
		}

		private Action<BotResult> AssertBotResult(SubTaskResult result)
		{
			return botResult
				=>
			{
				Assert.NotNull(botResult);
				Assert.NotNull(botResult.AdditionalData);
				Assert.False(botResult.Crashed);
				Assert.Equal(result, botResult.CompilationResult);
			};
		}

		[Fact]
		public async Task CancelDuringCompilation()
		{
			GameModuleMock.SetupCompiler()
				.Returns(GameModuleEntryPointResult.Success)
				.Throws<OperationCanceledException>();

			var result = await MatchExecutor.ExecuteAsync(Request);

			Assert.Equal(JobStatus.Timeout, result.JobStatus);
			Assert.Equal(SubTaskResult.Unknown, result.ExecutionResult);

			Assert.Collection(result.BotResults,
				AssertBotResult(SubTaskResult.Ok),
				AssertBotResult(SubTaskResult.Aborted));
		}

		[Fact]
		public async Task ExecutesMatchSuccessfully()
		{
			GameModuleMock.SetupCompiler()
				.Returns(GameModuleEntryPointResult.Success)
				.Returns(GameModuleEntryPointResult.Success);
			GameModuleMock.SetupExecutor(GameModuleEntryPointResult.Success,
				ToSuccessMatchResult(Request));

			var result = await MatchExecutor.ExecuteAsync(Request);
			AssertResultSuccess(result);

			Assert.All(result.BotResults, AssertBotResult(SubTaskResult.Ok));
		}

		[Fact]
		public async Task ReportsCompilationFailure()
		{
			GameModuleMock.SetupCompiler()
				.Returns(GameModuleEntryPointResult.Success)
				.Returns(GameModuleEntryPointResult.Failure);

			var result = await MatchExecutor.ExecuteAsync(Request);

			Assert.Equal(JobStatus.Error, result.JobStatus);

			// was not executed
			Assert.Equal(SubTaskResult.Unknown, result.ExecutionResult);

			Assert.Collection(result.BotResults,
				AssertBotResult(SubTaskResult.Ok),
				AssertBotResult(SubTaskResult.NotOk));
		}

		[Fact]
		public async Task ReportsRuntimeFailure()
		{
			GameModuleMock.SetupCompiler()
				.Returns(GameModuleEntryPointResult.Success)
				.Returns(GameModuleEntryPointResult.Success);
			GameModuleMock.SetupExecutor().Returns(GameModuleEntryPointResult.ModuleError);

			var result = await MatchExecutor.ExecuteAsync(Request);

			Assert.All(result.BotResults, AssertBotResult(SubTaskResult.Ok));

			Assert.Equal(JobStatus.Error, result.JobStatus);
			Assert.Equal(SubTaskResult.ModuleError, result.ExecutionResult);
		}
	}
}