using OPCAIC.Infrastructure.Dtos.Tournaments;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OPCAIC.Infrastructure.Repositories
{
	public interface ITournamentParticipantRepository
	{
		Task<TournamentParticipantDto[]> GetParticipantsAsync(long tournamentId, CancellationToken cancellationToken = default);
		Task<bool> CreateAsync(long tournamentId, IEnumerable<string> emails, CancellationToken cancellationToken = default);
		Task<bool> DeleteAsync(long tournamentId, string email, CancellationToken cancellationToken = default);
	}
}
