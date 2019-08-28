using System.Threading;
using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json.Linq;
using OPCAIC.ApiService.IoC;
using OPCAIC.ApiService.Models.Tournaments;
using OPCAIC.ApiService.ModelValidationHandling;
using OPCAIC.ApiService.Services;
using OPCAIC.Infrastructure.Dtos.Tournaments;
using OPCAIC.Infrastructure.Repositories;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.ApiService.Test.Services
{
	public class TournamentServiceTest : ApiServiceTestBase
	{
		/// <inheritdoc />
		public TournamentServiceTest(ITestOutputHelper output) : base(output)
		{
			tournamentRepository = Services.Mock<ITournamentRepository>(MockBehavior.Strict);
			gameRepository = Services.Mock<IGameRepository>(MockBehavior.Strict);

			Services.AddMapper();
		}

		private readonly Mock<ITournamentRepository> tournamentRepository;
		private readonly Mock<IGameRepository> gameRepository;

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
			gameRepository.Setup(r => r.GetConfigurationSchemaAsync(gameId, CancellationToken))
				.ReturnsAsync(SomeSchema);

			await AssertBadRequest(ValidationErrorCodes.InvalidConfiguration,
				nameof(NewTournamentModel.Configuration),
				() => GetService<TournamentsService>()
					.CreateAsync(
						new NewTournamentModel
						{
							Name = "Tournament",
							GameId = gameId,
							Description = "",
							Configuration = JObject.Parse("{ 'Not': 'Correct' }")
						}, CancellationToken));
		}

		[Fact]
		public async Task Create_Success()
		{
			const long gameId = 1;
			gameRepository.Setup(r => r.GetConfigurationSchemaAsync(gameId, CancellationToken))
				.ReturnsAsync(SomeSchema);

			tournamentRepository.Setup(r => r.CreateAsync(It.IsAny<NewTournamentDto>(), CancellationToken)).ReturnsAsync(1);

			await GetService<TournamentsService>()
				.CreateAsync(
					new NewTournamentModel
					{
						Name = "Tournament",
						GameId = gameId,
						Description = "",
						Configuration = JObject.Parse(ValidJson)
					}, CancellationToken);
		}
	}
}