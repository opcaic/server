using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using OPCAIC.Application.Dtos.Matches;
using OPCAIC.Application.Dtos.Tournaments;
using OPCAIC.Application.Interfaces.MatchGeneration;
using OPCAIC.Domain.Enums;
using OPCAIC.Utils;

namespace OPCAIC.Application.Services.MatchGeneration
{
	/// <summary>
	///     Match generator for the double-elimination bracket tournament format.
	/// </summary>
	public class DoubleEliminationMatchGenerator : BracketMatchGenerator, IBracketsMatchGenerator
	{
		/// <inheritdoc />
		public DoubleEliminationMatchGenerator(ILogger<DoubleEliminationMatchGenerator> logger, IMatchTreeFactory matchTreeFactory) : base(logger, matchTreeFactory)
		{
		}

		/// <inheritdoc />
		public TournamentFormat Format => TournamentFormat.DoubleElimination;

		/// <inheritdoc />
		public (List<NewMatchDto> matches, bool done) Generate(
			TournamentBracketsGenerationDto tournament)
		{
			Require.That<ArgumentException>(Format == tournament.Format, "Wrong tournament format");
			var tree = matchTreeFactory.GetDoubleEliminationTree(tournament.ActiveSubmissionIds.Count);
			var queued = tournament.Matches.ToDictionary(m => m.Index);

			if (tree.Final == null)
			{
				return (new List<NewMatchDto>(), true); // edge case
			}

			var submissions = tournament.ActiveSubmissionIds;
			submissions.Sort();
			var matches = new List<NewMatchDto>(submissions.Count);

			var ctx = new BracketGenerationContext(submissions, matches, queued, tournament.Id, tournament.RankingStrategy);

			if (tree.ThirdPlaceMatch != null &&
				!queued.TryGetValue(tree.ThirdPlaceMatch.MatchIndex, out _))
			{
				ProcessNode(tree.ThirdPlaceMatch, ctx);
			}

			if (!queued.TryGetValue(tree.Final.MatchIndex, out var match))
			{
				ProcessNode(tree.Final, ctx);
			}
			else if (GetMatchWinner(match, tournament.RankingStrategy) != null)
			// all other matches are generated
			{
				// if final has been already finished, check if the additional match is needed
				if (tournament.ActiveSubmissionIds.Count > 2 && // no secondary final with only two players
					match.Submissions[0].Id != GetMatchWinner(match, tournament.RankingStrategy).Id &&
					queued.Count == tree.MatchNodesById.Count - 1)
				{
					// winner comes from losers bracket, and needs to win twice in order for the
					// tournament to be fair

					ProcessNode(tree.SecondaryFinal, ctx);
				}

				return (ctx.NewMatches, true); // last match
			}

			ProcessBrackets(tree.Levels, ctx);
			ProcessBrackets(tree.LosersLevels, ctx);

			return (matches, false); // we don't know we are finished until the final is played
		}
	}
}