using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OPCAIC.ApiService.Models;
using OPCAIC.ApiService.Models.Broker;

namespace OPCAIC.ApiService.Services
{
	public interface IBrokerService
	{
		Task<ResultModel> CancelWork(Guid id, CancellationToken cancellationToken);
		Task<ResultModel> CancelWork(string workerIdentity, CancellationToken cancellationToken);
		Task<ResultModel> PrioritizeWork(Guid id, CancellationToken cancellationToken);
		Task<BrokerStatsModel> GetStats(CancellationToken cancellationToken);

		Task<List<WorkItemModel>> GetWorkItems(WorkItemFilterModel filter,
			CancellationToken cancellationToken);
	}
}