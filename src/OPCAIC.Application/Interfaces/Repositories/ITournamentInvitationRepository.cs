using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Application.Specifications;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Application.Interfaces.Repositories
{
	public interface ITournamentInvitationRepository : IRepository<TournamentInvitation>
	{
		Task<bool> CreateAsync(long tournamentId, IEnumerable<string> emails,
			CancellationToken cancellationToken = default);

		Task<bool> DeleteAsync(long tournamentId, string email,
			CancellationToken cancellationToken = default);
	}
}