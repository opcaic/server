namespace OPCAIC.Infrastructure.Dtos.Documents
{
	public class DocumentFilterDto : FilterDtoBase
	{
		public const string SortByName = "name";
		public const string SortByCreated = "created";

		public string Name { get; set; }

		public long? TournamentId { get; set; }
	}
}