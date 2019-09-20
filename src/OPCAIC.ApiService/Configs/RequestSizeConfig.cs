namespace OPCAIC.ApiService.Configs
{
	public class RequestSizeConfig
	{
		public long MaxTournamentFileSize { get; set; } = 64 * 1024 * 1024;
		public long MaxSubmissionFileSize { get; set; } = 64 * 1024 * 1024;
		public long MaxResultFileSize { get; set; } = 64 * 1024 * 1024;
	}
}