using System.Threading.Tasks;

namespace OPCAIC.Infrastructure.Repository
{
	/// <summary>
	///   Provides most basic methods for any repository implementations.
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	public interface IRepository<TEntity>
	{
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