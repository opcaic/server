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
	///     Generator for the matrix of matches (each pair of submission will compete against each other
	///     in some match).
	/// </summary>
	public class TableMatchGenerator : IDeadlineMatchGenerator
	{
		/// <inheritdoc />
		public TournamentFormat Format => TournamentFormat.Table;

		/// <inheritdoc />
		public List<NewMatchDto> Generate(TournamentDeadlineGenerationDto tournament)
		{
			Require.That<ArgumentException>(Format == tournament.Format, "Wrong tournament format");
			var submissions = tournament.ActiveSubmissionIds;

			var matches = new List<NewMatchDto>(submissions.Count * (submissions.Count - 1) / 2);

			for (var i = 0; i < submissions.Count; i++)
			for (var j = i + 1; j < submissions.Count; j++)
			{
				matches.Add(new NewMatchDto
				{
					Index = matches.Count,
					TournamentId = tournament.Id,
					Submissions = new List<long>(2)
					{
						submissions[i],
						submissions[j]
					}
				});
			}

			return matches;
		}
	}
}