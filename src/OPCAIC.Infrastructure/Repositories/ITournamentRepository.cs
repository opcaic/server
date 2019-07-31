using System.Threading;
using System.Threading.Tasks;
using System.Xml.Xsl;
using OPCAIC.Infrastructure.Dtos;
using OPCAIC.Infrastructure.Entities;

namespace OPCAIC.Infrastructure.Repositories
{
	public interface ITournamentRepository : IRepository<Tournament>
	{
		/// <summary>
		///     Gets general information about all tournaments.
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task<TournamentInfoDto[]> GetAllTournamentsInfo(
			CancellationToken cancellationToken = default);

		/// <summary>
		///     Gets general information about the tournament with given id, returns null if no such tournament exists.
		/// </summary>
		/// <param name="id">Id of the tournament.</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task<TournamentInfoDto> GetAllTournamentInfo(long id,
			CancellationToken cancellationToken = default);

		/// <summary>
		///     Creates or updates tournament based on the general information about it.
		/// </summary>
		/// <param name="tournament">The tournament to be updated.</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task UpdateTournament(TournamentInfoDto tournament,
			CancellationToken cancellationToken = default);
	}
}
