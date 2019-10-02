using System.Threading.Tasks;
using Moq;
using OPCAIC.Application.Specifications;
using OPCAIC.Application.Test;
using OPCAIC.Application.Users.Commands;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.ApiService.Test.Users.Commands
{
	public class ChangeLanguageCommandTest : HandlerTest<ChangeLanguageCommand.Handler>
	{
		private readonly Mock<IRepository<User>> repository;
		/// <inheritdoc />
		public ChangeLanguageCommandTest(ITestOutputHelper output) : base(output)
		{
			repository = Services.Mock<IRepository<User>>();
		}

		[Fact]
		public Task ChangesLanguage()
		{
			repository.SetupUpdate((ChangeLanguageCommand.Handler.UpdateDto dto)
				=> dto.LocalizationLanguage == "cz", CancellationToken);

			return Handler.Handle(
				new ChangeLanguageCommand
				{
					Language = "cz",
					RequestingUserId = 1,
					RequestingUserRole = UserRole.User
				}, CancellationToken);
		}
	}
}