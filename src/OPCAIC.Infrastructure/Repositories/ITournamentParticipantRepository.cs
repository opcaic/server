using OPCAIC.Infrastructure.Dtos.Tournaments;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OPCAIC.Infrastructure.Repositories
{
	public interface ITournamentParticipantRepository
	{
		Task<TournamentParticipantDto[]> GetParticipantsAsync(long tournamentId, CancellationToken cancellationToken);
		Task<bool> CreateAsync(long tournamentId, IEnumerable<string> emails, CancellationToken cancellationToken);
		Task<bool> DeleteAsync(long tounamentId, string email, CancellationToken cancellationToken);
	}
}
