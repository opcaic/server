using System.Threading.Tasks;
using Moq;
using OPCAIC.ApiService.Users.Commands;
using OPCAIC.Application.Exceptions;
using OPCAIC.Application.Specifications;
using OPCAIC.Application.Test;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.ApiService.Test.Users.Commands
{
	public class UpdateUserCommandTest : HandlerTest<UpdateUserCommand.Handler>
	{
		private readonly Mock<IRepository<User>> repository;

		/// <inheritdoc />
		public UpdateUserCommandTest(ITestOutputHelper output) : base(output)
		{
			repository = Services.Mock<IRepository<User>>(MockBehavior.Strict);
		}

		[Fact]
		public Task ThrowsWhenChangingLastAdminRole()
		{
			var id = 15; // random id
			var command = new UpdateUserCommand
			{
				Id = id,
				RequestingUserId = id, // we try to change our own role
				Role = UserRole.User,
				RequestingUserRole = UserRole.Admin,
			};

			repository.SetupExists(false, CancellationToken);

			return Should.ThrowAsync<BusinessException>(() => Handler.Handle(command, CancellationToken));
		}
	}
}