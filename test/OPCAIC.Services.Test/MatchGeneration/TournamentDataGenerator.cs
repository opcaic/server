using System.Linq;
using OPCAIC.Infrastructure.Entities;

namespace OPCAIC.Services.Test
{
	public static class TournamentDataGenerator
	{
		public static Tournament Generate(int participants) => new Tournament
		{
			Id = 1,
			Submissions = Enumerable.Range(0, participants).Select(i => new Submission()
			{
				Author = i.ToString(),
				IsActive = true
			}).ToList()
		};
	}
}