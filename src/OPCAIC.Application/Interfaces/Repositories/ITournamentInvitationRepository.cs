using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Application.Dtos;
using OPCAIC.Application.Dtos.Tournaments;

namespace OPCAIC.Application.Interfaces.Repositories
{
	public interface ITournamentInvitationRepository
	{
		Task<ListDto<TournamentInvitationDto>> GetInvitationsAsync(long tournamentId,
			TournamentInvitationFilterDto filter, CancellationToken cancellationToken = default);

		Task<bool> CreateAsync(long tournamentId, IEnumerable<string> emails,
			CancellationToken cancellationToken = default);

		Task<bool> DeleteAsync(long tournamentId, string email,
			CancellationToken cancellationToken = default);
	}
}