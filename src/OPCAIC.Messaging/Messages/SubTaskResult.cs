namespace OPCAIC.Messaging.Messages
{
	/// <summary>
	///     Represents a result of a sub-task.
	/// </summary>
	public enum SubTaskResult
	{
		// !!! Do not reorder the members, their value should always be in ascending order of severity

		/// <summary>
		///     Unknown, task wasn't run.
		/// </summary>
		Unknown,

		/// <summary>
		///     Sub-task finished successfully with positive result.
		/// </summary>
		Ok,

		/// <summary>
		///     Sub-task finished successfully with negative result.
		/// </summary>
		NotOk,

		/// <summary>
		///     Execution of the sub-task was aborted.
		/// </summary>
		Aborted,

		/// <summary>
		///     Sub-task Failed due to responsible module.
		/// </summary>
		ModuleError,

		/// <summary>
		///     Execution of the sub-task failed  due to error in the platform.
		/// </summary>
		PlatformError
	}
}