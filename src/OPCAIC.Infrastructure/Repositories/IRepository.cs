using System.Threading;
using System.Threading.Tasks;

namespace OPCAIC.Infrastructure.Repositories
{
	/// <summary>
	///   Provides most basic methods for any repository implementations.
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	public interface IRepository<TEntity>
	{
		/// <summary>
		///   Checks that entity with given id exists.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default);

		/// <summary>
		///   Checks that entity with given id exists.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		bool Exists(long id);

		/// <summary>
		///   Deletes given entity from the repository.
		/// </summary>
		/// <param name="entity"></param>
		void Delete(TEntity entity);

		/// <summary>
		///   Adds a new entity to the repository.
		/// </summary>
		/// <param name="entity"></param>
		void Add(TEntity entity);

		/// <summary>
		///   Saves all changes made.
		/// </summary>
		void SaveChanges();

		/// <summary>
		///   Saves all changes made.
		/// </summary>
		Task SaveChangesAsync();
	}
}