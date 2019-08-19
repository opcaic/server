using System.Threading;
using System.Threading.Tasks;

namespace OPCAIC.Infrastructure.Repositories
{
	public interface IDeleteRepository
	{
		Task<bool> DeleteAsync(long id, CancellationToken cancellationToken);
	}
}