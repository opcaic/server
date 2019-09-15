using System.Linq;
using OPCAIC.Application.Dtos.Tournaments;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.Test.MatchGeneration
{
	public static class TournamentDataGenerator
	{
		/// <summary>
		///     Generates a tournament for tests purposes. Participants have ascending ids from 0 to N,
		///     the Authors name is same as id.
		/// </summary>
		/// <param name="participants">Number of participants in the tournament.</param>
		/// <param name="format">Format of the tournament.</param>
		/// <returns></returns>
		public static TournamentDeadlineGenerationDto Generate(int participants, TournamentFormat format)
		{
			return new TournamentDeadlineGenerationDto
			{
				Id = 1,
				Format = format,
				ActiveSubmissionIds = Enumerable.Range(0, participants).Select(i => (long)i).ToList(),
			};
		}
	}
}