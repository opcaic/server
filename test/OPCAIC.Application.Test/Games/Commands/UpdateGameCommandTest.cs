using System.Threading;
using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json.Linq;
using OPCAIC.Application.Exceptions;
using OPCAIC.Application.Games.Commands;
using OPCAIC.Application.Infrastructure.Validation;
using OPCAIC.Application.Specifications;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.Application.Test.Games.Commands
{
	public class UpdateGameCommandTest : HandlerTest<UpdateGameCommand.Handler>
	{
		
		private readonly Mock<IRepository<Game>> repository;
		/// <inheritdoc />
		public UpdateGameCommandTest(ITestOutputHelper output) : base(output)
		{
			repository = Services.Mock<IRepository<Game>>();
		}

		private readonly UpdateGameCommand Command = new UpdateGameCommand
		{
			Id = 1,
			Name = "Game",
			Type = GameType.TwoPlayer,
			ConfigurationSchema = JObject.Parse(SomeSchema),
			MaxAdditionalFilesSize = 1000,
			Key = "game"
		};

		[Fact]
		public async Task NameConflict()
		{
			repository.SetupExists(true, CancellationToken);

			var ex = await Should.ThrowAsync<ConflictException>(
				Handler.Handle(Command, CancellationToken));

			ex.ErrorCode.ShouldBe(ValidationErrorCodes.GameNameConflict);
			ex.Field.ShouldBe(nameof(Game.Name));
		}

		[Fact]
		public async Task NotFound()
		{
			repository.SetupExists(false, CancellationToken);
			repository.SetupUpdate((UpdateGameCommand g) => true, CancellationToken, false);

			var command = new UpdateGameCommand();
			var ex = await Should.ThrowAsync<NotFoundException>(
				Handler.Handle(command, CancellationToken));

			ex.Resource.ShouldBe(nameof(Game));
			ex.ResourceId.ShouldBe(command.Id);
		}

		[Fact]
		public Task Success()
		{
			repository.SetupExists(false, CancellationToken);
			repository.SetupUpdate((UpdateGameCommand g) => true, CancellationToken);

			return Handler.Handle(new UpdateGameCommand(), CancellationToken);
		}

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
		public void InvalidSchema()
		{
			// setup invalid schema
			Command.ConfigurationSchema = JObject.Parse("{ 'some': 'object'}");

			// no setup needed
			var res = Validate(Command);

			res.IsValid.ShouldBeFalse();
			var error = res.Errors.ShouldHaveSingleItem();
			error.ErrorCode.ShouldBe(ValidationErrorCodes.InvalidSchema);
		}
	}
}