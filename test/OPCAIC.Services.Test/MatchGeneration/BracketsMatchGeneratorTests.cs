using System.Collections.Generic;
using System.Linq;
using Moq;
using OPCAIC.Infrastructure.Entities;
using OPCAIC.Infrastructure.Repositories;
using OPCAIC.TestUtils;
using Xunit;
using Xunit.Abstractions;
using Match = OPCAIC.Infrastructure.Entities.Match;

namespace OPCAIC.Services.Test
{
	public class BracketsMatchGeneratorTests : ServiceTestBase
	{
		private BracketsMatchGenerator generator;
		private Mock<IMatchRepository> matchRepository;

		/// <inheritdoc />
		public BracketsMatchGeneratorTests(ITestOutputHelper output) : base(output)
		{
			matchRepository = Services.Mock<IMatchRepository>();
			generator = GetService<BracketsMatchGenerator>();
		}

		[Fact]
		public void SimpleTest()
		{
			Tournament tournament = TournamentDataGenerator.Generate(4);

			matchRepository.Setup(r => r.AllMatchesFromTournament(It.IsAny<long>()))
				.Returns(Enumerable.Empty<Match>());

			var (matches, done) = generator.Generate(tournament);

			Assert.False(done); // always done
			Assert.Equal(2, matches.Count()); // bottom matches

			foreach (var match in matches)
			{
				Assert.Equal(MatchState.Waiting, match.MatchState);
				Assert.Equal(2, match.Participants.Count);
				Assert.Contains(string.Join("+", match.Participants.Select(p => p.Author)),
					new[] {"0+3", "1+2"});
			}
		}

		[Fact]
		public void SimpleUnevenTest()
		{
			Tournament tournament = TournamentDataGenerator.Generate(6);
			List<Match> mockMatches = new List<Match>();

			matchRepository.Setup(r => r.AllMatchesFromTournament(It.IsAny<long>()))
				.Returns(mockMatches);

			var (matches, done) = generator.Generate(tournament);

			Assert.False(done); // always done
			Assert.Equal(2, matches.Count()); // bottom matches

			foreach (var match in matches)
			{
				Assert.Equal(MatchState.Waiting, match.MatchState);
				Assert.Equal(2, match.Participants.Count);
				Assert.Contains(string.Join("+", match.Participants.Select(p => p.Author)),
					new[] {"3+4", "2+5"});
			}

			// TODO: simulate tournament progress
		}
	}
}
