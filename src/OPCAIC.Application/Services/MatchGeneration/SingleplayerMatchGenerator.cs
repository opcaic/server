using System;
using System.Collections.Generic;
using System.Linq;
using OPCAIC.Application.Dtos.Matches;
using OPCAIC.Application.Dtos.Tournaments;
using OPCAIC.Application.Interfaces.MatchGeneration;
using OPCAIC.Domain.Enums;
using OPCAIC.Utils;

namespace OPCAIC.Application.Services.MatchGeneration
{
	/// <summary>
	///     Match generator for the single player games (generates a "match" per submission).
	/// </summary>
	public class SinglePlayerMatchGenerator : IDeadlineMatchGenerator
	{
		/// <inheritdoc />
		public TournamentFormat Format => TournamentFormat.SinglePlayer;

		/// <inheritdoc />
		public List<NewMatchDto> Generate(TournamentDeadlineGenerationDto tournament)
		{
			Require.That<ArgumentException>(Format == tournament.Format, "Wrong tournament format");
			var i = 0;

			return tournament.ActiveSubmissionIds.Select(s => new NewMatchDto
			{
				Index = i++,
				TournamentId = tournament.Id,
				Submissions = new List<long>(1) { s }
			}).ToList();
		}
	}
}