using System.Linq;
using OPCAIC.Domain.Enums;
using OPCAIC.Infrastructure.Dtos.Tournaments;
using Shouldly;
using Xunit;

namespace OPCAIC.Services.Test.MatchGeneration
{
	public class TableMatchGeneratorTest
	{
		private readonly TableMatchGenerator generator = new TableMatchGenerator();

		private readonly TournamentDeadlineGenerationDto tournament = TournamentDataGenerator.Generate(3, TournamentFormat.Table);

		[Fact]
		public void SimpleTest()
		{
			var matches = generator.Generate(tournament);

			matches.Count.ShouldBe(3);
			foreach (var match in matches)
			{
				match.Submissions.Count.ShouldBe(2);
				new[] { "0+1", "0+2", "1+2" }.ShouldContain(
					string.Join("+", match.Submissions));
			}
		}
	}
}