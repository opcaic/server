using System.Threading;
using System.Threading.Tasks;

namespace OPCAIC.Application.Interfaces.Repositories
{
	public interface IDeleteRepository
	{
		Task<bool> DeleteAsync(long id, CancellationToken cancellationToken);
	}
}