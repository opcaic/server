using System.Collections.Generic;

namespace OPCAIC.ApiService.Models
{
	public class ListModel<T> where T: class
	{
		public IEnumerable<T> List { get; set; }

		public int Total { get; set; }
	}
}
