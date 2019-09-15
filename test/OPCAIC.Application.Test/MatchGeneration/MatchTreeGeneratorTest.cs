using System.Collections.Generic;
using System.Linq;
using OPCAIC.Application.Services;
using OPCAIC.TestUtils;
using Xunit;

namespace OPCAIC.Application.Test.MatchGeneration
{
	public class MatchTreeGeneratorTest
	{
		public static int[] Participants = {1, 2, 3, 8, 14};

		public static MatrixTheoryData<int, bool> SingleEliminationData
			=> new MatrixTheoryData<int, bool>(Participants, new[] {true, false});

		public static IEnumerable<object[]> DoubleEliminationData
			=> Participants.Select(i => new object[] {i});

		[Theory]
		[MemberData(nameof(SingleEliminationData))]
		public void SingleEliminationMatchCount(int participants, bool doThirdPlace)
		{
			var tree = MatchTreeGenerator.GenerateSingleElimination(participants, doThirdPlace);

			// each match eliminates one participant, therefore there need to be N-1 matches to get the
			// winner, plus additional one to get a single third place
			var expectedCount = doThirdPlace && participants > 3
				? participants
				: participants - 1;

			Assert.Equal(expectedCount, tree.MatchNodesById.Count);
		}

		[Theory]
		[MemberData(nameof(DoubleEliminationData))]
		public void DoubleEliminationMatchCount(int participants)
		{
			var tree = MatchTreeGenerator.GenerateDoubleElimination(participants);

			// N-1 matches for winners bracket, N-2 matches for losers bracket + additional one for
			// winners of both brackets (optional rematch is not included in the tree)
			var expectedCount = participants - 1 + (participants - 2) + 1;

			Assert.Equal(expectedCount, tree.MatchNodesById.Count);
		}
	}
}