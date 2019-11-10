using System.Collections.Generic;

namespace OPCAIC.Application.Infrastructure
{
	public struct PagedResult<T>
	{
		public PagedResult(int totalCount, List<T> list)
		{
			Total = totalCount;
			List = list;
		}

		public int Total { get; set; }
		public List<T> List { get; set; }
	}
}