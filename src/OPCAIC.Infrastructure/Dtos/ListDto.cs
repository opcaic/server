using System.Collections.Generic;

namespace OPCAIC.Infrastructure.Dtos
{
	public class ListDto<T> where T: class
	{
		public IEnumerable<T> List { get; set; }

		public int Total { get; set; }
	}
}
