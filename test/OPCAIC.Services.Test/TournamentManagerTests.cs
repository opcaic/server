using OPCAIC.Infrastructure.Repositories;
using OPCAIC.Services.Extensions;
using OPCAIC.TestUtils;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.Services.Test
{
	public class TournamentManagerTests : ServiceTestBase
	{
		/// <inheritdoc />
		public TournamentManagerTests(ITestOutputHelper output) : base(output)
		{
			Services.AddMatchGenerators();
			Services.Mock<IMatchRepository>();
		}

		[Fact]
		public void ResolvesIEnumerable()
		{
			var generators = GetService<TournamentManager>();
		}
	}
}