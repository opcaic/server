using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using OPCAIC.ApiService.Models;
using OPCAIC.ApiService.Models.Broker;
using OPCAIC.Broker;

namespace OPCAIC.ApiService.Services
{
	public class BrokerService : IBrokerService
	{
		private readonly IMapper mapper;
		private readonly IBroker broker;

		public BrokerService(IMapper mapper, IBroker broker)
		{
			this.mapper = mapper;
			this.broker = broker;
		}

		/// <inheritdoc />
		public async Task CancelWork(Guid id, CancellationToken cancellationToken)
		{
			await broker.CancelWork(id);
		}

		/// <inheritdoc />
		public async Task PrioritizeWork(Guid id, CancellationToken cancellationToken)
		{
			await broker.PrioritizeWork(id);
		}

		/// <inheritdoc />
		public async Task<BrokerStatsModel> GetStats(CancellationToken cancellationToken)
		{
			var stats = await broker.GetStats();
			return mapper.Map<BrokerStatsModel>(stats);
		}

		/// <inheritdoc />
		public async Task<ListModel<WorkItemModel>> GetWorkItems(WorkItemFilterModel filter, CancellationToken cancellationToken)
		{
			var filterDto = mapper.Map<WorkItemFilterDto>(filter);
			var workItemsDto = await broker.FilterWork(filterDto);
			var workItems = workItemsDto.Select(wi => mapper.Map<WorkItemModel>(wi));
			return new ListModel<WorkItemModel>
			{
				Total = workItemsDto.Count,
                List = workItems.ToList()
			};
		}
	}
}
