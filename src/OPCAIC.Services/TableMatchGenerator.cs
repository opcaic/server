using System.Collections.Generic;
using System.Linq;
using OPCAIC.Infrastructure.Entities;
using OPCAIC.Infrastructure.Enums;

namespace OPCAIC.Services
{
	/// <summary>
	///     Generator for the matrix of matches (each pair of submission will compete against each other
	///     in some match).
	/// </summary>
	public class TableMatchGenerator : IMatchGenerator
	{
		/// <inheritdoc />
		public TournamentFormat Format => TournamentFormat.Table;
		
		/// <inheritdoc />
		public (IEnumerable<Match> matches, bool done) Generate(Tournament tournament)
		{
			var submissions = tournament.GetActiveSubmissions().ToList();

			IList<Match> matches = new List<Match>(submissions.Count * (submissions.Count - 1) / 2);

			for (var i = 0; i < submissions.Count; i++)
			for (var j = i + 1; j < submissions.Count; j++)
			{
				matches.Add(new Match
				{
					Index = matches.Count,
					Tournament = tournament,
					Participations = new[]
					{
						new SubmissionParticipation {Submission = submissions[i]},
						new SubmissionParticipation {Submission = submissions[j]}
					}
				});
			}

			return (matches, true);
		}
	}
}