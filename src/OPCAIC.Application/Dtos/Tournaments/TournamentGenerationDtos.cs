using System;
using System.Collections.Generic;
using OPCAIC.Application.Dtos.Matches;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.Dtos.Tournaments
{
	public abstract class TournamentGenerationDtoBase
	{
		public long Id { get; set; }
		public TournamentFormat Format { get; set; }
		public List<long> ActiveSubmissionIds { get; set; }
	}

	public class TournamentBracketsGenerationDto : TournamentGenerationDtoBase
	{
		public List<MatchDetailDto> Matches { get; set; }
	}

	public class TournamentDeadlineGenerationDto : TournamentGenerationDtoBase
	{
	}

	public class TournamentOngoingGenerationDto : TournamentGenerationDtoBase
	{
		public int MatchesCount { get; set; }
		
		public DateTime EvaluationStarted { get; set; }

		public int MatchesPerDay { get; set; }
	}
}