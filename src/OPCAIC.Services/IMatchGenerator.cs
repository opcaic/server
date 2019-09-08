using System.Collections.Generic;
using OPCAIC.Infrastructure.Dtos.Matches;
using OPCAIC.Infrastructure.Dtos.Tournaments;

namespace OPCAIC.Services
{
	/// <summary>
	///     Defines methods for generating matches for individual categories of tournaments.
	/// </summary>
	public interface IMatchGenerator
	{
		(List<NewMatchDto> matches, bool done) GenerateBrackets(
			TournamentBracketsGenerationDto tournament);

		List<NewMatchDto> GenerateDeadline(TournamentDeadlineGenerationDto tournament);

		List<NewMatchDto> GenerateOngoing(TournamentOngoingGenerationDto tournament, int count);
	}
}