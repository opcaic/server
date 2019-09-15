using System.Threading;
using System.Threading.Tasks;

namespace OPCAIC.Infrastructure.Repositories
{
	public interface ILookupRepository<TDetailDto>
	{
		/// <summary>
		///     Checks whether entity with given id exists.
		/// </summary>
		/// <param name="id">Id of the entity.</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task<bool> ExistsByIdAsync(long id, CancellationToken cancellationToken);

		/// <summary>
		///     Returns detailed dto of the entity with given id.
		/// </summary>
		/// <param name="id">Id of the entity.</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task<TDetailDto> FindByIdAsync(long id, CancellationToken cancellationToken);
	}
}