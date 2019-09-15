using OPCAIC.Application.Dtos.Tournaments;
using OPCAIC.Application.Services;
using OPCAIC.Domain.Enums;
using Shouldly;
using Xunit;

namespace OPCAIC.Application.Test.MatchGeneration
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