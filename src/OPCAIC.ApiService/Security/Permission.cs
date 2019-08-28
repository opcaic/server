﻿namespace OPCAIC.ApiService.Security
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
		Create,
		Download,
		Search,
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