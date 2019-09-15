using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using OPCAIC.ApiService.Interfaces;
using OPCAIC.ApiService.Models;
using OPCAIC.ApiService.Models.Broker;
using OPCAIC.Application.Dtos.Broker;
using OPCAIC.Broker;

namespace OPCAIC.ApiService.Services
{
	public class BrokerService : IBrokerService
	{
		private readonly IBroker broker;
		private readonly IMapper mapper;

		public BrokerService(IMapper mapper, IBroker broker)
		{
			this.mapper = mapper;
			this.broker = broker;
		}

		/// <inheritdoc />
		public async Task<BrokerStatsModel> GetStats(CancellationToken cancellationToken)
		{
			var stats = await broker.GetStats();
			return mapper.Map<BrokerStatsModel>(stats);
		}

		/// <inheritdoc />
		public async Task<List<WorkItemModel>> GetWorkItems(WorkItemFilterModel filter,
			CancellationToken cancellationToken)
		{
			var items = await broker.GetWorkItems();
			return FilterWorkItems(filter, items).Select(wi => mapper.Map<WorkItemModel>(wi)).ToList();
		}

		/// <inheritdoc />
		public async Task<ResultModel> CancelWork(Guid id, CancellationToken cancellationToken)
		{
			return new ResultModel {Result = await broker.CancelWork(id)};
		}

		/// <inheritdoc />
		public async Task<ResultModel> PrioritizeWork(Guid id, CancellationToken cancellationToken)
		{
			return new ResultModel {Result = await broker.PrioritizeWork(id)};
		}

		private List<WorkItemDto> FilterWorkItems(WorkItemFilterModel filter, IEnumerable<WorkItem> workItems)
		{
			var filtered = workItems;
			if (filter.Since != null)
			{
				filtered = filtered.Where(wi => wi.QueuedTime >= filter.Since);
			}

			if (filter.Until != null)
			{
				filtered = filtered.Where(wi => wi.QueuedTime <= filter.Until);
			}

			if (filter.Game != null)
			{
				filtered = filtered.Where(wi => wi.Payload.Game == filter.Game);
			}

			return filtered.Select(wi => mapper.Map<WorkItemDto>(wi)).ToList();
		}
	}
}