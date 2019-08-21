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
		Join,
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
		DownloadSubmission,
	}

	public enum MatchPermission
	{
		ViewMatch,
		QueueMatchExecution,
	}
}