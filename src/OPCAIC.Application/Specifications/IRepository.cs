using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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

		Task SaveChangesAsync(CancellationToken cancellationToken);
	}
}