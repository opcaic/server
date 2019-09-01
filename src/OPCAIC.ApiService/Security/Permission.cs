namespace OPCAIC.ApiService.Security
{
	public enum TournamentPermission
	{
		Create,
		Read,
		Update,
		Delete,
		Search,
		Submit,
		ManageInvites,
		Join,
		EditDocument,
		DownloadAdditionalFiles,
		UploadAdditionalFiles
	}

	public enum UserPermission
	{
		Read,
		Update,
		Search,
	}

	public enum GamePermission
	{
		Create,
		Read,
		Update,
		Delete,
		Search
	}

	public enum EmailPermission
	{
		Read
	}

	public enum DocumentPermission
	{
		Create,
		Read,
		Update,
		Delete,
		Search
	}

	public enum SubmissionPermission
	{
		Read,
		Update,
		Download,
		Search,
		QueueValidation,
	}

	public enum SubmissionValidationPermission
	{
		UploadResult,
		DownloadResult,
	}

	public enum MatchPermission
	{
		Read,
		QueueMatchExecution,
		Search
	}

	public enum MatchExecutionPermission
	{
		UploadResult,
		DownloadResults
	}
}