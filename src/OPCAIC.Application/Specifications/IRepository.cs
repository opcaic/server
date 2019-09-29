using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Application.Infrastructure;

namespace OPCAIC.Application.Specifications
{
	public interface IRepository<T>
	{
		Task<List<T>> ListAsync(ISpecification<T> specification,
			CancellationToken cancellationToken);

		Task<List<TDestination>> ListAsync<TDestination>(
			IProjectingSpecification<T, TDestination> specification,
			CancellationToken cancellationToken);

		Task<PagedResult<T>> ListPagedAsync(ISpecification<T> specification,
			CancellationToken cancellationToken);

		Task<PagedResult<TDestination>> ListPagedAsync<TDestination>(
			IProjectingSpecification<T, TDestination> specification,
			CancellationToken cancellationToken);

		Task<T> FindAsync(ISpecification<T> specification, CancellationToken cancellationToken);

		Task<TDestination> FindAsync<TDestination>(
			IProjectingSpecification<T, TDestination> specification,
			CancellationToken cancellationToken);

		Task<bool> UpdateAsync<TDto>(ISpecification<T> specification, TDto dto,
			CancellationToken cancellationToken);

		Task<bool> ExistsAsync(ISpecification<T> specification,
			CancellationToken cancellationToken);

		/// <summary>
		///     Creates entity and enforces it's creation in the database.
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task CreateAsync(T entity, CancellationToken cancellationToken);

		/// <summary>
		///     Adds entity to be added to the database when <see cref="SaveChangesAsync" /> is called.
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		void Add(T entity);

		/// <summary>
		///     Deletes a single entity described by given specification.
		/// </summary>
		/// <param name="specification">Specification of the entity.</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task<bool> DeleteAsync(ISpecification<T> specification, CancellationToken cancellationToken);

		/// <summary>
		///     Deletes a single entity from the repository.
		/// </summary>
		/// <param name="entity">The entity to delete</param>
		/// <returns></returns>
		void Delete(T entity);

		/// <summary>
		///     Saves all changes made to the entities.
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task SaveChangesAsync(CancellationToken cancellationToken);
	}
}