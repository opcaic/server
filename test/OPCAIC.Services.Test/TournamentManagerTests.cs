using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using OPCAIC.Infrastructure.Extensions;
using OPCAIC.Infrastructure.Repositories;
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
