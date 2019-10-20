using System.Collections.Generic;
using System.Linq;
using OPCAIC.Application.Services;
using OPCAIC.Application.Services.MatchGeneration;
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

			// the number of generated matches is:
			var expectedCount = 0;

			// N-1 matches in winner bracket (the final is not included in winner bracket)
			expectedCount += participants - 1;

			// N-2 matches in losers bracket
			expectedCount += participants - 2;

			// 1 for (primary) final
			expectedCount++;

			// 1 for third place match (if more than 3 players)
			expectedCount += participants > 3 ? 1 : 0;

			// 1 (optional) secondary final
			expectedCount += participants > 2 ? 1 : 0;

			// override for trivial cases
			expectedCount = (participants) switch
			{
				1 => 0,
				2 => 1,
				_ => expectedCount
			};

		Assert.Equal(expectedCount, tree.MatchNodesById.Count);
		}
	}
}