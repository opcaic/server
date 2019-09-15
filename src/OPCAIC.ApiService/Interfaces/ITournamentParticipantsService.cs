using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OPCAIC.ApiService.Models;
using OPCAIC.ApiService.Models.Tournaments;

namespace OPCAIC.ApiService.Interfaces
{
	public interface ITournamentParticipantsService
	{
		Task<ListModel<TournamentParticipantPreviewModel>> GetParticipantsAsync(long tournamentId, TournamentParticipantFilter filter, CancellationToken cancellationToken);
		Task CreateAsync(long tournamentId, IEnumerable<string> emails, CancellationToken cancellationToken);
		Task DeleteAsync(long tounamentId, string email, CancellationToken cancellationToken);
	}
}