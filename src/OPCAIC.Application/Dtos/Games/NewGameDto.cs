namespace OPCAIC.Application.Dtos.Games
{
	public class NewGameDto
	{
		public string Name { get; set; }

		public string Key { get; set; }

		public string ConfigurationSchema { get; set; }
		
		public long MaxAdditionalFilesSize { get; set; }
	}
}