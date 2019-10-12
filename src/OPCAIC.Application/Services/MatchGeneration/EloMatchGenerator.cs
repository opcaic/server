using System;
using System.Collections.Generic;
using OPCAIC.Application.Dtos.Matches;
using OPCAIC.Application.Dtos.Tournaments;
using OPCAIC.Application.Interfaces.MatchGeneration;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.Services.MatchGeneration
{
	internal class EloMatchGenerator : IOngoingMatchGenerator
	{
		/// <inheritdoc />
		public TournamentFormat Format => TournamentFormat.Elo;

		/// <inheritdoc />
		public List<NewMatchDto> Generate(TournamentOngoingGenerationDto tournament, int count)
		{
			var rand = new Random();
			var matches = new List<NewMatchDto>(count);
			var submissions = tournament.Submissions.Count;

			if (submissions < 2)
			{
				return matches;
			}

			int matchesGenerated = 0;
			var pools = GeneratePlayersPools(tournament);
			while (matchesGenerated < count)
			{
				var pickPool = rand.NextDouble();
				var pool = pools[0];

				if (pickPool < 0.33) pool = pools[0];
				else if (pickPool < 0.66) pool = pools[1];
				else pool = pools[2];

				if (pool.Count < 2) continue;

				var sub1 = pool[rand.Next(pool.Count)];
				// make sure the two submissions are different;
				long sub2;
				do
				{
					sub2 = pool[rand.Next(pool.Count)];
				} while (sub1 == sub2);

				matches.Add(new NewMatchDto
				{
					Index = tournament.MatchesCount + matches.Count,
					Submissions = new List<long> {sub1, sub2},
					TournamentId = tournament.Id
				});
				matchesGenerated++;
			}

			return matches;
		}

		/// <summary>
		///     Divides players into 3 pools by their expected skill. The system is inspired by the one used by League of Legends,
		///     see: https://leagueoflegends.fandom.com/wiki/Elo_rating_system
		/// </summary>
		/// <returns> Three pools of submission ids, divided by their expected skill.</returns>
		private List<List<long>> GeneratePlayersPools(TournamentOngoingGenerationDto tournament)
		{
			var pools = new List<List<long>>();
			// "bronze division", elo 0 - 1149
			var bronze = new List<long>();
			// "silver division", elo 1150 - 1499
			var silver = new List<long>();
			// "gold division", elo above 1500
			var gold = new List<long>();

			var submissions = tournament.Submissions;
			foreach (var submission in submissions)
			{
				if (submission.Score < 1150) bronze.Add(submission.Id);
				else if (submission.Score < 1500) silver.Add(submission.Id);
				else gold.Add(submission.Id);
			}

			pools.Add(bronze);
			pools.Add(silver);
			pools.Add(gold);
			return pools;
		}
	}
}