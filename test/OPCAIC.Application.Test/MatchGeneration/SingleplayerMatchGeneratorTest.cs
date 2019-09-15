using System.Linq;
using OPCAIC.Application.Dtos.Tournaments;
using OPCAIC.Application.Services;
using OPCAIC.Application.Services.MatchGeneration;
using OPCAIC.Domain.Enums;
using Shouldly;
using Xunit;

namespace OPCAIC.Application.Test.MatchGeneration
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