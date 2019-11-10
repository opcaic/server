using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OPCAIC.ApiService.Models;
using OPCAIC.ApiService.Models.Broker;
using OPCAIC.Application.Dtos.Broker;

namespace OPCAIC.ApiService.Interfaces
{
	public interface IBrokerService
	{
		Task<ResultModel> CancelWork(Guid id, CancellationToken cancellationToken);
		Task<ResultModel> PrioritizeWork(Guid id, CancellationToken cancellationToken);
		Task<BrokerStatsModel> GetStats(CancellationToken cancellationToken);

		Task<List<WorkItemDto>> GetWorkItems(WorkItemFilterModel filter,
			CancellationToken cancellationToken);
	}
}