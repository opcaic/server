using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OPCAIC.Application.Interfaces.Repositories
{
	public interface IAsyncRepository<T> where T : class
	{
		Task<TResult> QueryAsync<TResult>(Func<IQueryable<T>, IQueryable<TResult>> query, CancellationToken cancellationToken);
	}
}