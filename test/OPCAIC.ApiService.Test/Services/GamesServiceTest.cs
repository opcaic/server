using Moq;
using Newtonsoft.Json.Linq;
using OPCAIC.ApiService.IoC;
using OPCAIC.ApiService.Models.Games;
using OPCAIC.ApiService.ModelValidationHandling;
using OPCAIC.ApiService.Services;
using OPCAIC.Application.Dtos;
using OPCAIC.Application.Dtos.Games;
using OPCAIC.Application.Infrastructure;
using OPCAIC.Application.Infrastructure.Validation;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.ApiService.Test.Services
{
	public class GamesServiceTest : ApiServiceTestBase
	{
		/// <inheritdoc />
		public GamesServiceTest(ITestOutputHelper output) : base(output)
		{
			gameRepository = Services.Mock<IGameRepository>(MockBehavior.Strict);

			Services.AddMapper();
		}

		private readonly Mock<IGameRepository> gameRepository;

		private const string GameName = "Game";
		private const long GameId = 1;
		private const string GameKey = "Key";
		private const string GameDefaultColor = "#FFFFFF";
		private const GameType GameDefaultType = GameType.SinglePlayer;

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
			gameRepository.Setup(r => r.ExistsOtherByNameAsync(GameName, null, CancellationToken))
				.ReturnsAsync(true);

			await AssertConflict(ValidationErrorCodes.GameNameConflict, nameof(NewGameModel.Name),
				() => GetService<GamesService>()
					.CreateAsync(
						new NewGameModel { Name = GameName }, CancellationToken));
		}

		[Fact]
		public async Task Create_Success()
		{
			gameRepository.Setup(r => r.ExistsOtherByNameAsync(GameName, null, CancellationToken))
				.ReturnsAsync(false);

			gameRepository.Setup(r => r.CreateAsync(It.IsAny<NewGameDto>(), CancellationToken))
				.ReturnsAsync(1);

			await GetService<GamesService>()
				.CreateAsync(
					new NewGameModel
					{
						Name = GameName,
						Key = GameKey,
						DefaultTournamentThemeColor = GameDefaultColor,
						Type = GameDefaultType
					}, CancellationToken);
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
		public async Task GetById_Success()
		{
			gameRepository.Setup(r => r.FindByIdAsync(GameId, CancellationToken))
				.ReturnsAsync(new GameDetailDto { ConfigurationSchema = "{}" });

			await GetService<GamesService>()
				.GetByIdAsync(GameId, CancellationToken);
		}


		[Fact]
		public async Task UpdateAsync_InvalidSchema()
		{
			gameRepository.Setup(r => r.ExistsOtherByNameAsync(GameName, null, CancellationToken))
				.ReturnsAsync(false);

			await AssertBadRequest(ValidationErrorCodes.InvalidSchema,
				nameof(UpdateGameModel.ConfigurationSchema), () => GetService<GamesService>()
					.UpdateAsync(GameId,
						new UpdateGameModel
						{
							Name = GameName,
							ConfigurationSchema = JObject.Parse("{ 'some': 'object'}")
						}, CancellationToken));
		}

		[Fact]
		public async Task UpdateAsync_NameConflict()
		{
			gameRepository.Setup(r => r.ExistsOtherByNameAsync(GameName, GameId, CancellationToken))
				.ReturnsAsync(true);

			await AssertConflict(ValidationErrorCodes.GameNameConflict,
				nameof(UpdateGameModel.Name), () => GetService<GamesService>()
					.UpdateAsync(GameId, new UpdateGameModel { Name = GameName }, CancellationToken));
		}

		[Fact]
		public async Task UpdateAsync_Success()
		{
			gameRepository.Setup(r => r.ExistsOtherByNameAsync(GameName, GameId, CancellationToken))
				.ReturnsAsync(false);
			gameRepository.Setup(r => r.UpdateAsync(GameId,
					It.Is<UpdateGameDto>(g => g.Name == GameName), CancellationToken))
				.ReturnsAsync(true);

			await GetService<GamesService>()
				.UpdateAsync(GameId,
					new UpdateGameModel
					{
						Name = GameName,
						ConfigurationSchema = JObject.Parse(SomeSchema)
					}, CancellationToken);
		}
	}
}