using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Infrastructure.Dtos;
using OPCAIC.Infrastructure.Dtos.Tournaments;

namespace OPCAIC.Infrastructure.Repositories
{
	public interface ITournamentParticipantRepository
	{
		Task<ListDto<TournamentParticipantDto>> GetParticipantsAsync(long tournamentId,
			TournamentParticipantFilterDto filter, CancellationToken cancellationToken = default);

		Task<bool> CreateAsync(long tournamentId, IEnumerable<string> emails,
			CancellationToken cancellationToken = default);

		Task<bool> DeleteAsync(long tournamentId, string email,
			CancellationToken cancellationToken = default);
	}
}