namespace OPCAIC.Infrastructure.Dtos.Games
{
	public class UpdateGameDto
	{
		public string Name { get; set; }

		public string Key { get; set; }

		public string ConfigurationSchema { get; set; }

		public long MaxAdditionalFilesSize { get; set; }
	}
}