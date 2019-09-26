using OPCAIC.Application.Infrastructure;

namespace OPCAIC.Application.Dtos
{
	public abstract class FilterDtoBase : PublicRequest
	{
		public int Offset { get; set; }

		public int Count { get; set; }

		public string SortBy { get; set; }

		public bool Asc { get; set; }
	}
}