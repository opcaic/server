namespace OPCAIC.Infrastructure.Dtos
{
	public abstract class FilterDtoBase
	{
		public int Offset { get; set; }

		public int Count { get; set; }

		public string SortBy { get; set; }

		public bool Asc { get; set; }
	}
}