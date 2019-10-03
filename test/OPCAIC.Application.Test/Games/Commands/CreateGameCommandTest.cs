using System.Threading.Tasks;
using Moq;
using OPCAIC.Application.Exceptions;
using OPCAIC.Application.Games.Commands;
using OPCAIC.Application.Infrastructure.Validation;
using OPCAIC.Application.Specifications;
using OPCAIC.Domain.Entities;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.Application.Test.Games.Commands
{
	public class CreateGameCommandTest : HandlerTest<CreateGameCommand.Handler>
	{
		private readonly Mock<IRepository<Game>> repository;
		/// <inheritdoc />
		public CreateGameCommandTest(ITestOutputHelper output) : base(output)
		{
			repository = Services.Mock<IRepository<Game>>();
		}

		[Fact]
		public async Task NameConflict()
		{
			repository.SetupExists(true, CancellationToken);

			var ex = await Should.ThrowAsync<ConflictException>(
				Handler.Handle(new CreateGameCommand(), CancellationToken));

			ex.ErrorCode.ShouldBe(ValidationErrorCodes.GameNameConflict);
			ex.Field.ShouldBe(nameof(Game.Name));
		}

		[Fact]
		public Task Success()
		{
			repository.SetupExists(false, CancellationToken);
			repository.Setup(r => r.CreateAsync(It.IsAny<Game>(), CancellationToken));

			return Handler.Handle(new CreateGameCommand(), CancellationToken);
		}
	}
}