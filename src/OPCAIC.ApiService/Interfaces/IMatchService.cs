using System.Threading;
using System.Threading.Tasks;
using OPCAIC.ApiService.Models;
using OPCAIC.ApiService.Models.Matches;

namespace OPCAIC.ApiService.Interfaces
{
	public interface IMatchService
	{
		Task<ListModel<MatchDetailModel>> GetByFilterAsync(MatchFilterModel filter,
			CancellationToken cancellationToken);

		Task<MatchDetailModel> GetByIdAsync(long id, CancellationToken cancellationToken);
	}
}