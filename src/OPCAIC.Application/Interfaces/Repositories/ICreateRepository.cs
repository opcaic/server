using System.Threading;
using System.Threading.Tasks;

namespace OPCAIC.Application.Interfaces.Repositories
{
	public interface ICreateRepository<in TNewDto>
	{
		/// <summary>
		///     Creates a new entity from given DTO. Returns id of the new entity.
		/// </summary>
		/// <param name="dto">DTO from which to create the new entity.</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task<long> CreateAsync(TNewDto dto, CancellationToken cancellationToken);
	}
}