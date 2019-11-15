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
		DownloadAdditionalFiles,
		ManageAdditionalFiles,
		StartEvaluation,
		PauseEvaluation,
		UnpauseEvaluation,
		StopEvaluation,
		Publish,
		ReadManagers,
		ReadAdmin,
		ManageManagers,
		ManageDocuments
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
		Read,
		Update,
		Delete,
		Search
	}

	public enum SubmissionPermission
	{
		Read,
		ReadAdmin,
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
		Search,
		ReadAdmin
	}

	public enum MatchPermission
	{
		Read,
		ReadAdmin,
		QueueMatchExecution,
		Search
	}

	public enum MatchExecutionPermission
	{
		Read,
		ReadAdmin,
		UploadResult,
		DownloadResults,
		Search
	}
}