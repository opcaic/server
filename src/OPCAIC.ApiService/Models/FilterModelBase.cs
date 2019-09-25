using Microsoft.AspNetCore.Mvc.ModelBinding;
using OPCAIC.Application.Infrastructure;

namespace OPCAIC.ApiService.Models
{
	public abstract class FilterModelBase : UserRequest
	{
		public int Offset { get; set; }

		public int Count { get; set; }

		public string SortBy { get; set; }

		public bool Asc { get; set; }
	}
}