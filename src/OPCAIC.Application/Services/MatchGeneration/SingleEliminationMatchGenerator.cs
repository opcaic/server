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
	///     Match generator for the single-elimination (single third place) bracket tournament format.
	/// </summary>
	public class SingleEliminationMatchGenerator : BracketMatchGenerator, IBracketsMatchGenerator
	{
		/// <inheritdoc />
		public SingleEliminationMatchGenerator(ILogger<SingleEliminationMatchGenerator> logger,
			IMatchTreeFactory matchTreeFactory)
			: base(logger, matchTreeFactory)
		{
		}

		/// <inheritdoc />
		public TournamentFormat Format => TournamentFormat.SingleElimination;

		/// <inheritdoc />
		public (List<NewMatchDto> matches, bool done) Generate(
			TournamentBracketsGenerationDto tournament)
		{
			Require.That<ArgumentException>(Format == tournament.Format, "Wrong tournament format");
			var tree =
				matchTreeFactory.GetSingleEliminationTree(tournament.ActiveSubmissionIds.Count, true);

			var queued = tournament.Matches.ToDictionary(m => m.Index);
			var submissions = tournament.ActiveSubmissionIds;
			submissions.Sort();
			var matches = new List<NewMatchDto>(submissions.Count);

			var ctx = new BracketGenerationContext(submissions, matches, queued, tournament.Id, tournament.RankingStrategy);

			ProcessBrackets(tree.Levels, ctx);

			return (matches, tree.MatchNodesById.Count == queued.Count + matches.Count);
		}
	}
}