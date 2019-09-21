using System.Collections.Generic;

namespace OPCAIC.Application.Specifications
{
	public struct PagedResult<T>
	{
		public PagedResult(int totalCount, List<T> list)
		{
			Total = totalCount;
			List = list;
		}

		public int Total { get; }
		public List<T> List { get; }
	}
}