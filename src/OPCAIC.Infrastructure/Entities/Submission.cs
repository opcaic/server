namespace OPCAIC.Infrastructure.Entities
{
	public class Submission : SoftDeletableEntity
	{
		public string Path { get; set; }
		public string Author { get; set; }
		public bool IsActive { get; set; }
	}
}