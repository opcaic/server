using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OPCAIC.Infrastructure.Entities;

namespace OPCAIC.Services
{
	internal static class TournamentExtensions
	{
		/// <summary>
		///     Gets active submissions registered in the match.
		/// </summary>
		/// <param name="tournament">The tournament.</param>
		/// <returns></returns>
		public static IEnumerable<Submission> GetActiveSubmissions(this Tournament tournament)
		{
			Debug.Assert(tournament != null);
			return tournament.Submissions.Where(s => s.IsActive);
		}
	}
}