using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Application.Dtos;
using OPCAIC.Application.Dtos.Tournaments;

namespace OPCAIC.Application.Interfaces.Repositories
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