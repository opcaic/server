using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OPCAIC.Infrastructure.DbContexts;
using OPCAIC.Infrastructure.Entities;

namespace OPCAIC.Infrastructure.Repositories
{
	public class UserTournamentRepository
		: RepositoryBase<UserTournament>, IUserTournamentRepository
	{
		public UserTournamentRepository(DataContext context, IMapper mapper)
			: base(context, mapper)
		{
		}

		public Task<long[]> FindTournamentsByUserAsync(long userId,
			CancellationToken cancellationToken)
		{
			return DbSet
				.Where(row => row.UserId == userId)
				.Select(row => row.TournamentId)
				.ToArrayAsync(cancellationToken);
		}

		public Task CreateAsync(long userId, long tournamentId, CancellationToken cancellationToken)
		{
			var entity = new UserTournament {UserId = userId, TournamentId = tournamentId};

			DbSet.Add(entity);
			return SaveChangesAsync(cancellationToken);
		}
	}
}