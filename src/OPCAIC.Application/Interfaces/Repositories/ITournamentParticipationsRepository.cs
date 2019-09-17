using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Application.Dtos.TournamentParticipations;

namespace OPCAIC.Application.Interfaces.Repositories
{
	public interface ITournamentParticipationsRepository
	{
		Task<bool> SetActiveSubmission(long tournamentId, long userId, UpdateTournamentParticipationDto dto, CancellationToken cancellationToken);
	}
}