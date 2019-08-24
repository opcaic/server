using System;
using System.Threading;
using System.Threading.Tasks;
using OPCAIC.ApiService.Models;
using OPCAIC.ApiService.Models.Broker;

namespace OPCAIC.ApiService.Services
{
	public interface IBrokerService
	{
		Task CancelWork(Guid id, CancellationToken cancellationToken);
		Task PrioritizeWork(Guid id, CancellationToken cancellationToken);
		Task<BrokerStatsModel> GetStats(CancellationToken cancellationToken);

		Task<ListModel<WorkItemModel>> GetWorkItems(WorkItemFilterModel filter,
			CancellationToken cancellationToken);
	}
}