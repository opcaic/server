using System.Threading;
using System.Threading.Tasks;

namespace OPCAIC.Infrastructure.Repositories
{
	public interface IAuthDataRepository<TAuthDto>
	{
		/// <summary>
		///     Gets data needed for authorization decision on operations on entity identified by given id.
		/// </summary>
		/// <param name="id">Id of the entity.</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task<TAuthDto> GetAuthorizationData(long id, CancellationToken cancellationToken = default);
	}
}