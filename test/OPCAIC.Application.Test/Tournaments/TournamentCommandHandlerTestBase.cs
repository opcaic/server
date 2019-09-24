using Moq;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.TestUtils;
using Xunit.Abstractions;

namespace OPCAIC.Application.Test.Tournaments
{
	public class TournamentCommandHandlerTestBase : ServiceTestBase
	{
		protected readonly Mock<ITournamentRepository> repository;

		public TournamentCommandHandlerTestBase(ITestOutputHelper output) : base(output)
		{
			repository = Services.Mock<ITournamentRepository>();
		}
	}
}