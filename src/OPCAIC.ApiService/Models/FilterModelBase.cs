using System.ComponentModel.DataAnnotations;
using OPCAIC.ApiService.Validation;

namespace OPCAIC.ApiService.Models
{
	public abstract class FilterModelBase
	{
		[Required, MinValue(0)]
		public int Offset { get; set; }

		[Required, Range(1, 100)]
		public int Count { get; set; }

		[MinLength(1)]
		public string SortBy { get; set; }

		public bool Asc { get; set; }
	}
}