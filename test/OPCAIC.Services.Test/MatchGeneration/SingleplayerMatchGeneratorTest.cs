using System.Linq;
using OPCAIC.Infrastructure.Entities;
using Xunit;

namespace OPCAIC.Services.Test.MatchGeneration
{
	public class SingleplayerMatchGeneratorTest
	{
		private readonly SinglePlayerMatchGenerator generator = new SinglePlayerMatchGenerator();

		private readonly Tournament tournament = TournamentDataGenerator.Generate(3);

		[Fact]
		public void SimpleTest()
		{
			var (matches, done) = generator.Generate(tournament);

			Assert.True(done); // always done
			Assert.Equal(3, matches.Count());
			foreach (var match in matches)
			{
				Assert.Equal(1, match.Participations.Count);
			}

			Assert.Equal(new[] {"0", "1", "2"},
				matches.Select(m => m.Participations.Single().Submission.Author.FirstName));
		}
	}
}