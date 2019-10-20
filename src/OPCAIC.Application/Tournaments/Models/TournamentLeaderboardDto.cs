using System;
using System.Collections.Generic;
using AutoMapper;
using OPCAIC.Application.Dtos.Users;
using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.Tournaments.Models
{
	public class TournamentLeaderboardDto
	{
		public List<ParticipationDto> Participations { get; set; }
		public TournamentFormat Format { get; set; }
		public TournamentRankingStrategy RankingStrategy { get; set; }
		public bool Finished { get; set; }

		public class MatchDto
		{
			public Player FirstPlayer { get; set; }

			public Player SecondPlayer { get; set; }

			public long Id { get; set; }

			public long Index { get; set; }

			public DateTime? Executed { get; set; }

			public class Player
			{
				public long SubmissionId { get; set; }

				public long? OriginMatchIndex { get; set; }

				public double? Score { get; set; }
			}
		}

		public class ParticipationDto
		{
			public UserReferenceDto Author { get; set; }

			/// <summary>
			///     The meaning of this value depends on tournament format.
			/// </summary>
			public double SubmissionScore { get; set; }

			/// <summary>
			///     Placement in the results of the tournament.
			/// </summary>
			public int? Place { get; set; }

			/// <summary>
			///     Index of the slot in which the participant starts. The slots do not have to be continuous for bracket tournaments.
			/// </summary>
			public int StartingSlot { get; set; }

			/// <summary>
			///     Id of the submission which participated for this user.
			/// </summary>
			public long SubmissionId { get; set; }
		}
	}
}