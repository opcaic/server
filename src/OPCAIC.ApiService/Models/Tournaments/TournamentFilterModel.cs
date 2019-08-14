﻿using OPCAIC.ApiService.ModelValidationHandling.Attributes;
using OPCAIC.Infrastructure.Enums;

namespace OPCAIC.ApiService.Models.Tournaments
{
	public class TournamentFilterModel : FilterModelBase
	{
		[ApiMinLength(1)]
		public string Name { get; set; }

		public long? GameId { get; set; }

		public TournamentFormat? Format { get; set; }

		public TournamentScope? Scope { get; set; }

		public TournamentRankingStrategy? RankingStrategy { get; set; }
	}
}