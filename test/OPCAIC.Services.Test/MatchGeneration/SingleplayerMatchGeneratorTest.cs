using System.Linq;
using OPCAIC.Domain.Enums;
using OPCAIC.Infrastructure.Dtos.Tournaments;
using Shouldly;
using Xunit;

namespace OPCAIC.Services.Test.MatchGeneration
{
	public class SingleplayerMatchGeneratorTest
	{
		private readonly SinglePlayerMatchGenerator generator = new SinglePlayerMatchGenerator();

		private readonly TournamentDeadlineGenerationDto tournament = TournamentDataGenerator.Generate(3, TournamentFormat.SinglePlayer);

		[Fact]
		public void SimpleTest()
		{
			var matches = generator.Generate(tournament);

			matches.Count.ShouldBe(3);
			foreach (var match in matches)
			{
				match.Submissions.Count.ShouldBe(1);
			}

			matches.Select(m => m.Submissions.Single()).ShouldBe(new long[] {0,1,2});
		}
	}
}