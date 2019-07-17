using System.Threading.Tasks;
using OPCAIC.Infrastructure.Entities;

namespace OPCAIC.Infrastructure.Repositories
{
	public interface ISimpleLookupRepository<TEntity> where TEntity : Entity
	{
		/// <summary>
		///   Finds an entity with given id in database. Returns null if not found.
		/// </summary>
		/// <param name="id">Primary key of the entity.</param>
		/// <returns></returns>
		TEntity Find(long id);

		/// <summary>
		///   Finds an entity with given id in database. Returns null if not found.
		/// </summary>
		/// <param name="id">Primary key of the entity.</param>
		/// <returns></returns>
		Task<TEntity> FindAsync(long id);

		/// <summary>
		///   Deletes entity with given id from the database. Does nothing if entity does not exist.
		/// </summary>
		/// <param name="id"></param>
		void Delete(long id);
	}
}