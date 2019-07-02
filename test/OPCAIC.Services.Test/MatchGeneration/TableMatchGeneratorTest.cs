using System.Collections.Generic;
using System.Linq;
using OPCAIC.Infrastructure.Entities;
using Xunit;

namespace OPCAIC.Services.Test
{
	public class TableMatchGeneratorTest
	{
		private readonly TableMatchGenerator generator = new TableMatchGenerator();

		private readonly Tournament tournament = TournamentDataGenerator.Generate(3);

		[Fact]
		public void SimpleTest()
		{
			var (matches, done) = generator.Generate(tournament);

			Assert.True(done); // always done
			Assert.Equal(3, matches.Count());
			foreach (var match in matches)
			{
				Assert.Equal(MatchState.Waiting, match.MatchState);
				Assert.Equal(2, match.Participants.Count);
				Assert.Contains(string.Join("+", match.Participants.Select(p => p.Author)),
					new[] {"0+1", "0+2", "1+2"});
			}
		}
	}
}
