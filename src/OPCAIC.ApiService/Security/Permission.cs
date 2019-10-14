namespace OPCAIC.ApiService.Security
{
	public enum TournamentPermission
	{
		Create,
		Read,
		Update,
		Delete,
		Search,
		ViewLeaderboard,
		Submit,
		ManageInvites,
		Join,
		EditDocument,
		DownloadAdditionalFiles,
		UploadAdditionalFiles,
		StartEvaluation,
        PauseEvaluation,
        UnpauseEvaluation,
        StopEvaluation,
        Publish,
		ManageManagers
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
		Read,
		ManageTemplates
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
		ReadDetail,
		Search
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
		DownloadResults,
		ReadDetail,
		Search
	}
}