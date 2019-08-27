using OPCAIC.ApiService.Models.Tournaments;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OPCAIC.ApiService.Services
{
	public interface ITournamentParticipantsService
	{
		Task<TournamentParticipantPreviewModel[]> GetParticipantsAsync(long tournamentId, CancellationToken cancellationToken);
		Task CreateAsync(long tournamentId, IEnumerable<string> emails, CancellationToken cancellationToken);
		Task DeleteAsync(long tounamentId, string email, CancellationToken cancellationToken);
	}
}