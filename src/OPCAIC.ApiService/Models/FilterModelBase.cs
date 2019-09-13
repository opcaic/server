namespace OPCAIC.ApiService.Models
{
	public abstract class FilterModelBase
	{
		public int Offset { get; set; }

		public int Count { get; set; }

		public string SortBy { get; set; }

		public bool Asc { get; set; }
	}
}