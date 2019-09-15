using System.Collections.Generic;

namespace OPCAIC.Application.Dtos
{
	public class ListDto<T> where T : class
	{
		public IList<T> List { get; set; }

		public int Total { get; set; }
	}
}