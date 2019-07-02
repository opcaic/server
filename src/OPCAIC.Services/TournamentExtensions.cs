using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OPCAIC.Infrastructure.Entities;

namespace OPCAIC.Services
{
	internal static class TournamentExtensions
	{
		public static IEnumerable<Submission> GetActiveSubmissions(this Tournament tournament)
		{
			Debug.Assert(tournament != null);
			return tournament.Submissions.Where(s => s.IsActive);
		}
	}
}
