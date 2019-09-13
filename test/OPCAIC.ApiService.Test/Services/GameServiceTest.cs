using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json.Linq;
using OPCAIC.ApiService.IoC;
using OPCAIC.ApiService.Models.Games;
using OPCAIC.ApiService.ModelValidationHandling;
using OPCAIC.ApiService.Services;
using OPCAIC.Infrastructure.Dtos;
using OPCAIC.Infrastructure.Dtos.Games;
using OPCAIC.Infrastructure.Entities;
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
		private const long GameId = 1;

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

		[Fact]
		public async Task GetByFilter_Success()
		{
			gameRepository.Setup(r => r.GetByFilterAsync(It.IsAny<GameFilterDto>(), CancellationToken))
				.ReturnsAsync(new ListDto<GamePreviewDto> {Total = 1, List = new[]
				{
					new GamePreviewDto
					{
						Id=1,
						Name = "Name"
					}
				}});

			var filter = new GameFilterModel {Count = 1};
			await GetService<GamesService>().GetByFilterAsync(filter, CancellationToken);
		}

		[Fact]
		public async Task GetById_Success()
		{
			gameRepository.Setup(r => r.FindByIdAsync(GameId, CancellationToken))
				.ReturnsAsync(new GameDetailDto { ConfigurationSchema = "{}" });

			await GetService<GamesService>()
				.GetByIdAsync(GameId, CancellationToken);
		}

		[Fact]
		public async Task GetById_NotFound()
		{
			gameRepository.Setup(r => r.FindByIdAsync(GameId, CancellationToken))
				.ReturnsAsync(() => null);

			await AssertNotFound<Game>(GameId, () =>
				GetService<GamesService>()
					.GetByIdAsync(GameId, CancellationToken));
		}

		[Fact]
		public async Task UpdateAsync_Success()
		{
			gameRepository.Setup(r => r.ExistsByNameAsync(GameName, CancellationToken))
				.ReturnsAsync(false);
			gameRepository.Setup(r => r.UpdateAsync(GameId, It.Is<UpdateGameDto>(g => g.Name == GameName), CancellationToken))
				.ReturnsAsync(true);

			await GetService<GamesService>()
				.UpdateAsync(GameId, new UpdateGameModel() {Name = GameName}, CancellationToken);
		}

		[Fact]
		public async Task UpdateAsync_NameConflict()
		{
			gameRepository.Setup(r => r.ExistsByNameAsync(GameName, CancellationToken))
				.ReturnsAsync(true);

			await AssertConflict(ValidationErrorCodes.GameNameConflict, nameof(UpdateGameModel.Name), () => GetService<GamesService>()
				.UpdateAsync(GameId, new UpdateGameModel() {Name = GameName}, CancellationToken));
		}
	}
}