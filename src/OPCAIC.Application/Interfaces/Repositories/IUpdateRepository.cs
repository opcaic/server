using System.Threading;
using System.Threading.Tasks;

namespace OPCAIC.Application.Interfaces.Repositories
{
	public interface IUpdateRepository<in TUpdateDto>
	{
		/// <summary>
		///     Updates entity with given Id in the database.
		/// </summary>
		/// <param name="id">Id of the entity to be updated</param>
		/// <param name="dto">DTO with which to update the entity.</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task<bool> UpdateAsync(long id, TUpdateDto dto,
			CancellationToken cancellationToken);
	}
}