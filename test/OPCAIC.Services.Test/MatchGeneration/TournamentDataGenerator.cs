using System.Linq;
using OPCAIC.Infrastructure.Entities;

namespace OPCAIC.Services.Test
{
	public static class TournamentDataGenerator
	{
		/// <summary>
		///   Generates a tournament for tests purposes. Participants have ascending ids from 0 to N,
		///   the Authors name is same as id.
		/// </summary>
		/// <param name="participants">Number of participants in the tournament.</param>
		/// <returns></returns>
		public static Tournament Generate(int participants) => new Tournament
		{
			Id = 1,
			Submissions = Enumerable.Range(0, participants).Select(i => new Submission()
			{
				Id = i,
				Author = i.ToString(),
				IsActive = true
			}).ToList()
		};
	}
}