using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json.Linq;
using OPCAIC.ApiService.IoC;
using OPCAIC.ApiService.Models.Games;
using OPCAIC.ApiService.ModelValidationHandling;
using OPCAIC.ApiService.Services;
using OPCAIC.Infrastructure.Dtos.Games;
using OPCAIC.Infrastructure.Repositories;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.ApiService.Test.Services
{
	public class GameServiceTest : ApiServiceTestBase
	{
		/// <inheritdoc />
		public GameServiceTest(ITestOutputHelper output) : base(output)
		{
			gameRepository = Services.Mock<IGameRepository>(MockBehavior.Strict);

			Services.AddMapper();
		}

		private readonly Mock<IGameRepository> gameRepository;

		private const string GameName = "Game";

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

		[Fact]
		public async Task Create_InvalidSchema()
		{
			gameRepository.Setup(r => r.ExistsByNameAsync(GameName, CancellationToken))
				.ReturnsAsync(false);

			await AssertBadRequest(ValidationErrorCodes.InvalidSchema,
				nameof(NewGameModel.ConfigurationSchema), () => GetService<GamesService>()
					.CreateAsync(
						new NewGameModel
						{
							Name = GameName,
							ConfigurationSchema = JObject.Parse("{ 'some': 'object'}")
						}, CancellationToken));
		}

		[Fact]
		public async Task Create_NameConflict()
		{
			gameRepository.Setup(r => r.ExistsByNameAsync(GameName, CancellationToken))
				.ReturnsAsync(true);

			await AssertConflict(ValidationErrorCodes.GameNameConflict, nameof(NewGameModel.Name),
				() => GetService<GamesService>()
					.CreateAsync(
						new NewGameModel
						{
							Name = GameName, ConfigurationSchema = JObject.Parse(SomeSchema)
						}, CancellationToken));
		}

		[Fact]
		public async Task Create_Success()
		{
			gameRepository.Setup(r => r.ExistsByNameAsync(GameName, CancellationToken))
				.ReturnsAsync(false);

			gameRepository.Setup(r => r.CreateAsync(It.IsAny<NewGameDto>(), CancellationToken))
				.ReturnsAsync(1);

			await GetService<GamesService>()
				.CreateAsync(
					new NewGameModel
					{
						Name = GameName, ConfigurationSchema = JObject.Parse(SomeSchema)
					}, CancellationToken);
		}
	}
}