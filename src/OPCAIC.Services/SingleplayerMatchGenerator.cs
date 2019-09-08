using System;
using System.Collections.Generic;
using System.Linq;
using OPCAIC.Infrastructure.Dtos.Matches;
using OPCAIC.Infrastructure.Dtos.Tournaments;
using OPCAIC.Infrastructure.Entities;
using OPCAIC.Infrastructure.Enums;
using OPCAIC.Utils;

namespace OPCAIC.Services
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