using System;
using System.Collections.Generic;
using System.Linq;

namespace OPCAIC.Broker
{
	public class WorkerInfoDto
	{
		public string Identity { get; set; }
		public Guid? CurrentJob { get; set; }
	}

	public class BrokerStatsDto
	{
		public List<WorkerInfoDto> Workers { get; set; }
	}

	public class WorkItemFilterDto
	{
		/// <summary>
		///     Since when (inclusive) we want to filter the items.
		/// </summary>
		public DateTime? Since { get; set; }

		/// <summary>
		///     Since when (inclusive) we want to filter the items.
		/// </summary>
		public DateTime? Until { get; set; }

		public string Game { get; set; }

		public List<WorkItem> Filter(IEnumerable<WorkItem> workItems)
		{
			var filtered = workItems;
			if (Since != null)
			{
				filtered = filtered.Where(wi => wi.QueuedTime >= Since);
			}

			if (Until != null)
			{
				filtered = filtered.Where(wi => wi.QueuedTime <= Until);
			}

			if (Game != null)
			{
				filtered = filtered.Where(wi => wi.Payload.Game == Game);
			}

			return filtered.ToList();
		}
	}
}