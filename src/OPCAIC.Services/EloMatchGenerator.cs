using System;
using System.Collections.Generic;
using OPCAIC.Infrastructure.Dtos.Matches;
using OPCAIC.Infrastructure.Dtos.Tournaments;
using OPCAIC.Infrastructure.Enums;

namespace OPCAIC.Services
{
	internal class EloMatchGenerator : IOngoingMatchGenerator
	{
		/// <inheritdoc />
		public TournamentFormat Format => TournamentFormat.Elo;

		/// <inheritdoc />
		public List<NewMatchDto> Generate(TournamentOngoingGenerationDto tournament, int count)
		{
			// TODO: implement true ELO algorithm

			var rand = new Random();
			var matches = new List<NewMatchDto>(count);
			var submissions = tournament.ActiveSubmissionIds.Count;

			if (submissions < 2)
			{
				return matches;
			}

			for (var i = 0; i < count; i++)
			{
				var sub1 = tournament.ActiveSubmissionIds[rand.Next(submissions)];

				// make sure the two submissions are different;
				long sub2;
				do
				{
					sub2 = tournament.ActiveSubmissionIds[rand.Next(submissions)];
				} while (sub1 == sub2);


				matches.Add(new NewMatchDto
				{
					Index = tournament.MatchesCount + matches.Count,
					Submissions = new List<long> {sub1, sub2},
					TournamentId = tournament.Id
				});
			}

			return matches;
		}
	}
}