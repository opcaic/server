namespace OPCAIC.GameModules.Interface
{
	/// <summary>
	///     Represents results of calling game module entry points.
	/// </summary>
	public enum GameModuleEntryPointResult
	{
		/// <summary>
		///     Unknown result, should never occur.
		/// </summary>
		Unknown,

		/// <summary>
		///     Stage of the game module finished successfully, the result of the stage is Success.
		/// </summary>
		Success,

		/// <summary>
		///     Stage of the game module finished successfully, the result of the stage is Failure.
		/// </summary>
		Failure,

		/// <summary>
		///     Stage of the game module failed during runtime.
		/// </summary>
		ModuleError
	}
}