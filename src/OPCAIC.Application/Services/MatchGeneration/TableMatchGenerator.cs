using System;
using System.Collections.Generic;
using OPCAIC.Application.Dtos.Matches;
using OPCAIC.Application.Dtos.Tournaments;
using OPCAIC.Application.Interfaces;
using OPCAIC.Domain.Enums;
using OPCAIC.Utils;

namespace OPCAIC.Application.Services
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