using System;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json.Linq;
using OPCAIC.ApiService.IoC;
using OPCAIC.Application.Infrastructure.Validation;
using OPCAIC.Application.Specifications;
using OPCAIC.Application.Tournaments.Commands;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;
using Shouldly;
using System.Threading.Tasks;
using OPCAIC.Common;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.Application.Test.Tournaments
{
	public class CreateTournamentCommandTest : HandlerTest<CreateTournamentCommand.Handler>
	{
		/// <inheritdoc />
		public CreateTournamentCommandTest(ITestOutputHelper output) : base(output)
		{
			tournamentRepository = Services.Mock<IRepository<Tournament>>(MockBehavior.Strict);
			gameRepository = Services.Mock<IRepository<Game>>(MockBehavior.Strict);
			Services.Mock<ITimeService>().SetupGet(g => g.Now).Returns(DateTime.UtcNow);

			Services.AddSingleton(gameRepository.Object);

			Services.AddMapper();
			Services.AddValidatorsFromAssembly(typeof(CreateTournamentCommand).Assembly);
		}

		private readonly Mock<IRepository<Tournament>> tournamentRepository;
		private readonly Mock<IRepository<Game>> gameRepository;

		private const string SomeSchema = @"{
  'title': 'A registration form',
  'description': 'A simple form example.',
  'type': 'object',
  'required': [
    'firstName',
    'lastName'
  ],
  'properties': {
    'firstName': {
      'type': 'string',
      'title': 'First name',
      'default': 'Chuck'
    },
    'lastName': {
      'type': 'string',
      'title': 'Last name'
    }
  }
}";

		private const string ValidJson = @"{
  'firstName': 'Ian',
  'lastName': 'Fist'
}";

		[Fact]
		public async Task Create_InvalidConfiguration()
		{
			const long gameId = 1;

			gameRepository.Setup(r
					=> r.FindAsync(It.IsAny<ProjectingSpecification<Game, string>>(),
						CancellationToken))
				.ReturnsAsync(SomeSchema);

			gameRepository
				.Setup(s => s.ExistsAsync(It.IsAny<BaseSpecification<Game>>(), CancellationToken))
				.ReturnsAsync(true);

			var instance = new CreateTournamentCommand
			{
				Name = "Tournament",
				GameId = gameId,
				Description = "",
				Configuration = JObject.Parse("{ 'Not': 'Correct' }"),
				Format = TournamentFormat.DoubleElimination,
				Scope = TournamentScope.Deadline,
				RankingStrategy = TournamentRankingStrategy.Maximum,
				MaxSubmissionSize = 1
			};

			var ctx = new ValidationContext(instance);
			ctx.SetServiceProvider(ServiceProvider);
			var result = GetService<IValidator<CreateTournamentCommand>>().Validate(ctx);
			result.IsValid.ShouldBeFalse();
			var error = result.Errors.ShouldHaveSingleItem();
			error.ErrorCode.ShouldBe(ValidationErrorCodes.InvalidConfiguration);
		}

		[Fact]
		public async Task Create_Success()
		{
			const long gameId = 1;

			gameRepository.Setup(r
					=> r.FindAsync(It.IsAny<ProjectingSpecification<Game, string>>(),
						CancellationToken))
				.ReturnsAsync(SomeSchema);

			gameRepository
				.Setup(s => s.ExistsAsync(It.IsAny<BaseSpecification<Game>>(), CancellationToken))
				.ReturnsAsync(true);

			tournamentRepository
				.Setup(r => r.CreateAsync(It.IsAny<Tournament>(), CancellationToken))
				.Returns(Task.CompletedTask);

			var instance = new CreateTournamentCommand
			{
				Name = "Tournament",
				GameId = gameId,
				Description = "",
				Configuration = JObject.Parse(ValidJson),
				Format = TournamentFormat.DoubleElimination,
				Scope = TournamentScope.Deadline,
				RankingStrategy = TournamentRankingStrategy.Maximum,
				MaxSubmissionSize = 1
			};

			var ctx = new ValidationContext(instance);
			ctx.SetServiceProvider(ServiceProvider);
			var result = GetService<IValidator<CreateTournamentCommand>>().Validate(ctx);
			result.IsValid.ShouldBeTrue();

			await Handler.Handle(instance, CancellationToken);
		}
	}
}