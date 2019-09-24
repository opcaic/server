using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Application.Dtos.TournamentParticipations;
using OPCAIC.Application.Specifications;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Application.Interfaces.Repositories
{
	public interface ITournamentParticipationsRepository : IRepository<TournamentParticipation>
	{
		Task<bool> SetActiveSubmission(long tournamentId, long userId, UpdateTournamentParticipationDto dto, CancellationToken cancellationToken);
	}
}